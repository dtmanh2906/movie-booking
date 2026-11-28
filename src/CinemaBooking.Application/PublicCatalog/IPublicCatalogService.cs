using CinemaBooking.Application.Common;
using CinemaBooking.Application.Showtimes;

namespace CinemaBooking.Application.PublicCatalog;

public interface IPublicCatalogService
{
    Task<PagedResult<MovieListItemDto>> GetMoviesAsync(
        MovieQueryParams queryParams, CancellationToken cancellationToken = default);

    Task<MovieDetailDto?> GetMovieBySlugAsync(
        string slug, CancellationToken cancellationToken = default);

    Task<List<ShowtimeDto>> GetShowtimesAsync(
        string slug, DateOnly? date = null, int? cinemaId = null,
        CancellationToken cancellationToken = default);
}
