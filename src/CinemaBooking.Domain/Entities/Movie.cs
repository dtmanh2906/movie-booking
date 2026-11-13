using CinemaBooking.Domain.Enums;

namespace CinemaBooking.Domain.Entities;

public class Movie
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Director { get; set; }

    public string Actors { get; set; } = string.Empty;

    public int Duration { get; set; }

    public DateOnly ReleaseDate { get; set; }

    public string MovieFormat { get; set; } = "2D";

    public string Language { get; set; } = "Tiếng Việt";

    public AgeRating AgeRating { get; set; } = AgeRating.P;

    public string? BannerUrl { get; set; }

    public string? PosterUrl { get; set; }

    public string? TrailerUrl { get; set; }

    public bool IsShowing { get; set; } = true;

    public ICollection<Genre> Genres { get; set; }
        = new List<Genre>();

    public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}