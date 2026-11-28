using CinemaBooking.Domain.Enums;

namespace CinemaBooking.Application.PublicCatalog;

public class MovieListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public int Duration { get; set; }
    public AgeRating AgeRating { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public List<string> Genres { get; set; } = new();
}

public class MovieDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Director { get; set; }
    public string Actors { get; set; } = string.Empty;
    public int Duration { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public string MovieFormat { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public AgeRating AgeRating { get; set; }
    public string? BannerUrl { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public List<string> Genres { get; set; } = new();
}
