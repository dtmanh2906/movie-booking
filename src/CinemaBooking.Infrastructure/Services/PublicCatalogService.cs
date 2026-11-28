using CinemaBooking.Application.Common;
using CinemaBooking.Application.PublicCatalog;
using CinemaBooking.Application.Showtimes;
using CinemaBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Services;

public class PublicCatalogService(AppDbContext db) : IPublicCatalogService
{
    private static readonly TimeSpan VietnamUtcOffset = TimeSpan.FromHours(7);

    public async Task<PagedResult<MovieListItemDto>> GetMoviesAsync(
        MovieQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(queryParams.Page, 1);
        var pageSize = Math.Clamp(queryParams.PageSize, 1, 50);
        var query = db.Movies.AsNoTracking().Where(m => m.IsShowing);

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var search = queryParams.Search.Trim();
            query = query.Where(m => m.Title.Contains(search));
        }

        if (queryParams.GenreId.HasValue)
        {
            var genreId = queryParams.GenreId.Value;
            query = query.Where(m => m.Genres.Any(g => g.Id == genreId));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var result = new PagedResult<MovieListItemDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        // Nhân bằng long để Page rất lớn không làm tràn int trước khi kiểm tra.
        var offset = (long)(page - 1) * pageSize;
        if (offset >= totalCount)
            return result;

        result.Items = await query
            .OrderByDescending(m => m.ReleaseDate)
            .ThenBy(m => m.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .Select(m => new MovieListItemDto
            {
                Id = m.Id,
                Title = m.Title,
                Slug = m.Slug,
                PosterUrl = m.PosterUrl,
                Duration = m.Duration,
                AgeRating = m.AgeRating,
                ReleaseDate = m.ReleaseDate,
                Genres = m.Genres.OrderBy(g => g.Name).Select(g => g.Name).ToList()
            })
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<MovieDetailDto?> GetMovieBySlugAsync(
        string slug, CancellationToken cancellationToken = default)
    {
        return await db.Movies.AsNoTracking()
            .Where(m => m.IsShowing && m.Slug == slug)
            .Select(m => new MovieDetailDto
            {
                Id = m.Id,
                Title = m.Title,
                Slug = m.Slug,
                Description = m.Description,
                Director = m.Director,
                Actors = m.Actors,
                Duration = m.Duration,
                ReleaseDate = m.ReleaseDate,
                MovieFormat = m.MovieFormat,
                Language = m.Language,
                AgeRating = m.AgeRating,
                BannerUrl = m.BannerUrl,
                PosterUrl = m.PosterUrl,
                TrailerUrl = m.TrailerUrl,
                Genres = m.Genres.OrderBy(g => g.Name).Select(g => g.Name).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<ShowtimeDto>> GetShowtimesAsync(
        string slug, DateOnly? date = null, int? cinemaId = null,
        CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTime.UtcNow;
        var query = db.Showtimes.AsNoTracking()
            .Where(s => s.Movie.Slug == slug && s.Movie.IsShowing
                && s.Room.IsActive && s.Room.Cinema.IsActive
                && !s.IsCancelled && s.StartTime > nowUtc);

        if (date.HasValue)
        {
            // Ngày Việt Nam [00:00, 24:00) tương ứng [17:00 hôm trước, 17:00) UTC.
            // Tính bằng ticks để cả DateOnly.MinValue/MaxValue không bị tràn khi đổi ngày.
            var startTicks = date.Value.ToDateTime(TimeOnly.MinValue).Ticks - VietnamUtcOffset.Ticks;
            var startUtc = new DateTime(Math.Max(startTicks, DateTime.MinValue.Ticks), DateTimeKind.Utc);
            var endUtc = new DateTime(startTicks + TimeSpan.TicksPerDay, DateTimeKind.Utc);
            query = query.Where(s => s.StartTime >= startUtc && s.StartTime < endUtc);
        }

        if (cinemaId.HasValue)
        {
            var id = cinemaId.Value;
            query = query.Where(s => s.Room.CinemaId == id);
        }

        return await query.OrderBy(s => s.StartTime).ThenBy(s => s.Id)
            .Select(s => new ShowtimeDto
            {
                Id = s.Id,
                MovieId = s.MovieId,
                MovieTitle = s.Movie.Title,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                TicketPrice = s.TicketPrice,
                RoomId = s.RoomId,
                RoomNumber = s.Room.RoomNumber,
                CinemaId = s.Room.CinemaId,
                CinemaName = s.Room.Cinema.Name,
                IsCancelled = s.IsCancelled
            })
            .ToListAsync(cancellationToken);
    }
}
