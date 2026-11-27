using CinemaBooking.Application.Exceptions;
using CinemaBooking.Application.Rooms;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Services;

public class RoomService(AppDbContext db) : IRoomService
{
    public async Task<List<RoomDto>> GetAllAsync(int? cinemaId = null, CancellationToken cancellationToken = default)
    {
        var query = db.Rooms.AsNoTracking();
        if (cinemaId.HasValue)
            query = query.Where(r => r.CinemaId == cinemaId.Value);

        var rooms = await query.OrderBy(r => r.CinemaId).ThenBy(r => r.RoomNumber)
            .ThenBy(r => r.Id).ToListAsync(cancellationToken);
        return rooms.Select(ToDto).ToList();
    }

    public async Task<RoomDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var room = await db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy phòng có id {id}.");
        return ToDto(room);
    }

    public async Task<RoomDto> CreateAsync(CreateRoomRequest request, CancellationToken cancellationToken = default)
    {
        var cinema = await db.Cinemas.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CinemaId, cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy rạp có id {request.CinemaId}.");
        if (!cinema.IsActive)
            throw new ConflictException("Không thể tạo phòng trong rạp đã vô hiệu hóa.");

        var number = request.RoomNumber.Trim();
        await EnsureUniqueNumberAsync(request.CinemaId, number, null, cancellationToken);
        var room = new Room
        {
            CinemaId = request.CinemaId, RoomNumber = number,
            RowCount = request.RowCount, SeatsPerRow = request.SeatsPerRow, IsActive = true
        };

        // Mỗi hàng tương ứng một chữ A–Z; số ghế bắt đầu từ 1.
        for (var row = 0; row < request.RowCount; row++)
        {
            for (var seat = 1; seat <= request.SeatsPerRow; seat++)
            {
                room.Seats.Add(new Seat
                {
                    SeatRow = ((char)('A' + row)).ToString(), SeatNumber = seat,
                    SeatType = SeatType.Standard, ExtraPrice = 0, IsActive = true
                });
            }
        }

        // EF lưu cả đồ thị Room–Seats trong một transaction của SaveChanges.
        db.Rooms.Add(room);
        await SaveRoomAsync(cancellationToken);
        return ToDto(room);
    }

    public async Task<RoomDto> UpdateAsync(int id, UpdateRoomRequest request, CancellationToken cancellationToken = default)
    {
        var room = await FindAsync(id, cancellationToken);
        var number = request.RoomNumber.Trim();
        await EnsureUniqueNumberAsync(room.CinemaId, number, id, cancellationToken);
        if (request.IsActive == false)
            await EnsureCanDeactivateAsync(id, cancellationToken);
        if (request.IsActive == true && !await db.Cinemas.AsNoTracking()
            .AnyAsync(c => c.Id == room.CinemaId && c.IsActive, cancellationToken))
            throw new ConflictException("Không thể kích hoạt phòng thuộc rạp đã vô hiệu hóa.");

        // Chỉ gán sau khi mọi kiểm tra thành công, tránh lưu một phần request bị từ chối.
        room.RoomNumber = number;
        room.IsActive = request.IsActive ?? room.IsActive;
        await SaveRoomAsync(cancellationToken);
        return ToDto(room);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var room = await FindAsync(id, cancellationToken);
        await EnsureCanDeactivateAsync(id, cancellationToken);
        room.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<SeatDto>> GetSeatsAsync(int roomId, CancellationToken cancellationToken = default)
    {
        await EnsureRoomExistsAsync(roomId, cancellationToken);
        var seats = await db.Seats.AsNoTracking().Where(s => s.RoomId == roomId)
            .OrderBy(s => s.SeatRow).ThenBy(s => s.SeatNumber).ToListAsync(cancellationToken);
        return seats.Select(ToDto).ToList();
    }

    public async Task<SeatDto> UpdateSeatAsync(int roomId, int seatId, UpdateSeatRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureRoomExistsAsync(roomId, cancellationToken);
        var seat = await db.Seats.FirstOrDefaultAsync(s => s.Id == seatId && s.RoomId == roomId, cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy ghế {seatId} trong phòng {roomId}.");
        // Giá trên BookingDetail là giá tại thời điểm đặt, không cập nhật theo ghế.
        seat.SeatType = request.SeatType!.Value;
        seat.ExtraPrice = request.ExtraPrice!.Value;
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(seat);
    }

    private async Task<Room> FindAsync(int id, CancellationToken cancellationToken) =>
        await db.Rooms.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
        ?? throw new NotFoundException($"Không tìm thấy phòng có id {id}.");

    private async Task EnsureRoomExistsAsync(int id, CancellationToken cancellationToken)
    {
        if (!await db.Rooms.AsNoTracking().AnyAsync(r => r.Id == id, cancellationToken))
            throw new NotFoundException($"Không tìm thấy phòng có id {id}.");
    }

    private async Task EnsureUniqueNumberAsync(int cinemaId, string number, int? excludeId, CancellationToken cancellationToken)
    {
        if (await db.Rooms.AsNoTracking().AnyAsync(r => r.CinemaId == cinemaId
            && r.RoomNumber == number && (!excludeId.HasValue || r.Id != excludeId.Value), cancellationToken))
            throw new ConflictException("Số phòng đã tồn tại trong rạp.");
    }

    private async Task EnsureCanDeactivateAsync(int id, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        if (await db.Showtimes.AsNoTracking().AnyAsync(s => s.RoomId == id
            && !s.IsCancelled && s.StartTime >= now, cancellationToken))
            throw new ConflictException("Không thể vô hiệu hóa phòng còn suất chiếu tương lai.");
    }

    private async Task SaveRoomAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql
            && sql.Number is 2601 or 2627
            && sql.Message.Contains("IX_Rooms_CinemaId_RoomNumber", StringComparison.Ordinal))
        {
            // Chỉ diễn giải lỗi của đúng unique index phòng; lỗi DB khác giữ nguyên.
            throw new ConflictException("Số phòng đã tồn tại trong rạp.");
        }
    }

    private static RoomDto ToDto(Room r) => new()
    {
        Id = r.Id, CinemaId = r.CinemaId, RoomNumber = r.RoomNumber,
        RowCount = r.RowCount, SeatsPerRow = r.SeatsPerRow, IsActive = r.IsActive
    };

    private static SeatDto ToDto(Seat s) => new()
    {
        Id = s.Id, RoomId = s.RoomId, SeatRow = s.SeatRow, SeatNumber = s.SeatNumber,
        SeatType = s.SeatType, ExtraPrice = s.ExtraPrice, IsActive = s.IsActive
    };
}
