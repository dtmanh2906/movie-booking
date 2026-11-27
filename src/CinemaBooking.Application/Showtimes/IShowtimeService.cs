namespace CinemaBooking.Application.Showtimes;

public interface IShowtimeService
{
    Task<List<ShowtimeDto>> GetAllAsync(int? movieId = null, int? cinemaId = null, int? roomId = null,
        DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken cancellationToken = default);
    Task<ShowtimeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ShowtimeDto> CreateAsync(CreateShowtimeRequest request, CancellationToken cancellationToken = default);
    Task<ShowtimeDto> UpdateAsync(int id, UpdateShowtimeRequest request, CancellationToken cancellationToken = default);
    Task CancelAsync(int id, CancellationToken cancellationToken = default);
}
