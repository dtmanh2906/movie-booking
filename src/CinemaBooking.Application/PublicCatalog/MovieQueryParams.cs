namespace CinemaBooking.Application.PublicCatalog;

public class MovieQueryParams
{
    public string? Search { get; set; }
    public int? GenreId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
