using System;
using System.Collections.Generic;
using CinemaBooking.Application.Genres;
using CinemaBooking.Domain.Enums;

namespace CinemaBooking.Application.Movies;

// DTO trả thông tin phim cho frontend
public class MovieDto
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
    public bool IsShowing { get; set; }

    public List<GenreDto> Genres { get; set; } = new();
}

// DTO nhận dữ liệu khi Admin thêm hoặc sửa phim
public class UpsertMovieRequest
{
    public string Title { get; set; } = string.Empty;
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

    public List<int> GenreIds { get; set; } = new();
}