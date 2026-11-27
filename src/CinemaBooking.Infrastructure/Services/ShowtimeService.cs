using CinemaBooking.Application.Exceptions;
using CinemaBooking.Application.Showtimes;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Services;

public class ShowtimeService(AppDbContext db) : IShowtimeService
{
    public async Task<List<ShowtimeDto>> GetAllAsync(int? movieId = null, int? cinemaId = null, int? roomId = null,
        DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken cancellationToken = default)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
            throw new BadRequestException("Thời gian từ phải nhỏ hơn hoặc bằng thời gian đến.");
        var query = db.Showtimes.AsNoTracking();
        if (movieId.HasValue) query = query.Where(s => s.MovieId == movieId.Value);
        if (cinemaId.HasValue) query = query.Where(s => s.Room.CinemaId == cinemaId.Value);
        if (roomId.HasValue) query = query.Where(s => s.RoomId == roomId.Value);
        if (from.HasValue)
        {
            var utc = from.Value.UtcDateTime;
            query = query.Where(s => s.StartTime >= utc);
        }
        if (to.HasValue)
        {
            var utc = to.Value.UtcDateTime;
            query = query.Where(s => s.StartTime <= utc);
        }
        return await Project(query.OrderBy(s => s.StartTime).ThenBy(s => s.Id)).ToListAsync(cancellationToken);
    }

    public async Task<ShowtimeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await Project(db.Showtimes.AsNoTracking().Where(s => s.Id == id)).FirstOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException($"Không tìm thấy suất chiếu có id {id}.");

    public async Task<ShowtimeDto> CreateAsync(CreateShowtimeRequest request, CancellationToken cancellationToken = default)
    {
        var movie = await db.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.Id == request.MovieId, cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy phim có id {request.MovieId}.");
        if (!movie.IsShowing) throw new ConflictException("Phim hiện không chiếu.");
        var room = await db.Rooms.AsNoTracking().Include(r => r.Cinema)
            .FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy phòng có id {request.RoomId}.");
        if (!room.IsActive || !room.Cinema.IsActive)
            throw new ConflictException("Phòng và rạp phải đang hoạt động.");

        EnsurePrice(request.TicketPrice);
        var start = request.StartTime.UtcDateTime;
        var end = CalculateEnd(start, movie.Duration);
        await EnsureNoOverlapAsync(request.RoomId, start, end, null, cancellationToken);
        var showtime = new Showtime
        {
            MovieId = request.MovieId, RoomId = request.RoomId,
            StartTime = start, EndTime = end, TicketPrice = request.TicketPrice
        };
        // Kiểm tra lại ngay trước khi lưu, không chỉ dựa vào validator đầu request.
        EnsureFuture(start);
        db.Showtimes.Add(showtime);
        await db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(showtime.Id, cancellationToken);
    }

    public async Task<ShowtimeDto> UpdateAsync(int id, UpdateShowtimeRequest request, CancellationToken cancellationToken = default)
    {
        var showtime = await FindAsync(id, cancellationToken);
        if (showtime.IsCancelled) throw new ConflictException("Không thể cập nhật suất chiếu đã hủy.");
        if (showtime.StartTime <= DateTime.UtcNow)
            throw new ConflictException("Không thể cập nhật suất chiếu đã bắt đầu.");
        if (!request.StartTime.HasValue && !request.TicketPrice.HasValue)
            throw new BadRequestException("Cần gửi StartTime hoặc TicketPrice.");
        if (request.TicketPrice.HasValue) EnsurePrice(request.TicketPrice.Value);

        var start = request.StartTime?.UtcDateTime ?? showtime.StartTime;
        var end = showtime.EndTime;
        if (start != showtime.StartTime)
        {
            // Mọi Booking, kể cả đã hết hạn/hủy, đều chặn thay đổi lịch theo phạm vi MVP.
            if (await db.Bookings.AsNoTracking().AnyAsync(b => b.ShowtimeId == id, cancellationToken))
                throw new ConflictException("Suất chiếu đã có Booking, chỉ được đổi giá vé.");
            var duration = await db.Movies.AsNoTracking().Where(m => m.Id == showtime.MovieId)
                .Select(m => m.Duration).SingleAsync(cancellationToken);
            end = CalculateEnd(start, duration);
            await EnsureNoOverlapAsync(showtime.RoomId, start, end, id, cancellationToken);
        }

        // Chỉ thay đổi entity sau tất cả kiểm tra; không chạm giá đã chốt trong Booking.
        if (showtime.StartTime <= DateTime.UtcNow)
            throw new ConflictException("Suất chiếu đã bắt đầu trong lúc xử lý.");
        EnsureFuture(start);
        showtime.StartTime = start;
        showtime.EndTime = end;
        showtime.TicketPrice = request.TicketPrice ?? showtime.TicketPrice;
        await db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task CancelAsync(int id, CancellationToken cancellationToken = default)
    {
        var showtime = await FindAsync(id, cancellationToken);
        if (await db.BookingDetails.AsNoTracking().AnyAsync(d => d.ShowtimeId == id && d.IsActive, cancellationToken))
            throw new ConflictException("Không thể hủy suất chiếu có vé đang hoạt động.");
        showtime.IsCancelled = true;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Showtime> FindAsync(int id, CancellationToken cancellationToken) =>
        await db.Showtimes.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
        ?? throw new NotFoundException($"Không tìm thấy suất chiếu có id {id}.");

    private async Task EnsureNoOverlapAsync(int roomId, DateTime start, DateTime end, int? excludeId, CancellationToken cancellationToken)
    {
        // Khoảng [start, end): cho phép hai suất nối tiếp nhau tại cùng một thời điểm.
        // Any rồi Save KHÔNG bảo đảm chống race giữa các request đồng thời (MVP).
        if (await db.Showtimes.AsNoTracking().AnyAsync(s => s.RoomId == roomId && !s.IsCancelled
            && (!excludeId.HasValue || s.Id != excludeId.Value)
            && start < s.EndTime && s.StartTime < end, cancellationToken))
            throw new ConflictException("Phòng đã có suất chiếu trùng thời gian.");
    }

    private static void EnsureFuture(DateTime start)
    {
        if (start <= DateTime.UtcNow) throw new BadRequestException("Thời gian bắt đầu phải ở tương lai.");
    }

    private static void EnsurePrice(decimal price)
    {
        if (!CreateShowtimeRequestValidator.ValidPrice(price))
            throw new BadRequestException("Giá vé phải là số nguyên dương, tối đa 12 chữ số.");
    }

    private static DateTime CalculateEnd(DateTime start, int duration)
    {
        if (duration is < 1 or > 500)
            throw new ConflictException("Thời lượng phim phải từ 1 đến 500 phút.");
        if (start > DateTime.MaxValue.AddMinutes(-duration))
            throw new BadRequestException("Thời gian kết thúc vượt phạm vi cho phép.");
        // MVP chưa áp dụng thời gian dọn phòng (cleaning buffer).
        return start.AddMinutes(duration);
    }

    private static IQueryable<ShowtimeDto> Project(IQueryable<Showtime> query) => query.Select(s => new ShowtimeDto
    {
        Id = s.Id, MovieId = s.MovieId, MovieTitle = s.Movie.Title,
        RoomId = s.RoomId, RoomNumber = s.Room.RoomNumber,
        CinemaId = s.Room.CinemaId, CinemaName = s.Room.Cinema.Name,
        StartTime = s.StartTime, EndTime = s.EndTime, TicketPrice = s.TicketPrice, IsCancelled = s.IsCancelled
    });
}
