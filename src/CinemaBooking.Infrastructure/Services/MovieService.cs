
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinemaBooking.Application.Common;
using CinemaBooking.Application.Exceptions;
using CinemaBooking.Application.Genres;
using CinemaBooking.Application.Movies;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Services;

public class MovieService(AppDbContext db) : IMovieService
{
    // 1. Lấy danh sách phim có phân trang và tìm kiếm
    public async Task<PagedResult<MovieDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? keyword)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = db.Movies
            .AsNoTracking()
            .Include(m => m.Genres)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            query = query.Where(m => m.Title.Contains(k));
        }

        var total = await query.CountAsync();

        var movies = await query
            .OrderByDescending(m => m.ReleaseDate)
            .ThenBy(m => m.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<MovieDto>
        {
            Items = movies.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    // 2. Lấy chi tiết phim theo ID
    public async Task<MovieDto> GetByIdAsync(int id)
    {
        var movie = await db.Movies
            .AsNoTracking()
            .Include(m => m.Genres)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new NotFoundException(
                $"Không tìm thấy phim có id {id}.");

        return ToDto(movie);
    }

    // 3. Thêm phim mới
    public async Task<MovieDto> CreateAsync(
        UpsertMovieRequest request)
    {
        var genres = await LoadGenresAsync(request.GenreIds);

        var movie = new Movie
        {
            Slug = await CreateUniqueSlugAsync(
                request.Title, null)
        };

        Apply(movie, request, genres);

        db.Movies.Add(movie);

        await db.SaveChangesAsync();

        return ToDto(movie);
    }

    // 4. Cập nhật thông tin phim
    public async Task<MovieDto> UpdateAsync(
        int id,
        UpsertMovieRequest request)
    {
        var movie = await db.Movies
            .Include(m => m.Genres)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new NotFoundException(
                $"Không tìm thấy phim có id {id}.");

        var genres = await LoadGenresAsync(request.GenreIds);

        // Chỉ tạo lại slug khi tên phim thay đổi
        if (!string.Equals(
            movie.Title,
            request.Title.Trim(),
            StringComparison.Ordinal))
        {
            movie.Slug = await CreateUniqueSlugAsync(
                request.Title, id);
        }

        Apply(movie, request, genres);

        await db.SaveChangesAsync();

        return ToDto(movie);
    }

    // 5. Ẩn phim thay vì xóa khỏi database
    public async Task HideAsync(int id)
    {
        // Tìm phim theo ID
        var movie = await db.Movies
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new NotFoundException(
                $"Không tìm thấy phim có id {id}.");

        // Đánh dấu phim không còn hiển thị
        movie.IsShowing = false;

        // Lưu thay đổi xuống SQL Server
        await db.SaveChangesAsync();
    }
    // 5. Kiểm tra các thể loại có tồn tại trong database
    private async Task<List<Genre>> LoadGenresAsync(
        List<int> ids)
    {
        // Loại bỏ ID trùng lặp
        var distinct = ids.Distinct().ToList();

        var genres = await db.Genres
            .Where(g => distinct.Contains(g.Id))
            .ToListAsync();

        if (genres.Count != distinct.Count)
        {
            throw new BadRequestException(
                "Có thể loại không tồn tại.");
        }

        return genres;
    }

    // 6. Tạo slug không trùng với các phim hiện có
    private async Task<string> CreateUniqueSlugAsync(
        string title,
        int? excludeId)
    {
        var baseSlug = SlugHelper.Slugify(title);
        var slug = baseSlug;
        var i = 2;

        while (await db.Movies.AnyAsync(
            m => m.Slug == slug && m.Id != excludeId))
        {
            slug = $"{baseSlug}-{i++}";
        }

        return slug;
    }

    // 7. Gán dữ liệu từ Request sang Entity Movie
    private static void Apply(
        Movie movie,
        UpsertMovieRequest r,
        List<Genre> genres)
    {
        movie.Title = r.Title.Trim();
        movie.Description = Clean(r.Description);
        movie.Director = Clean(r.Director);
        movie.Actors = r.Actors.Trim();
        movie.Duration = r.Duration;
        movie.ReleaseDate = r.ReleaseDate;
        movie.MovieFormat = r.MovieFormat.Trim();
        movie.Language = r.Language.Trim();
        movie.AgeRating = r.AgeRating;
        movie.BannerUrl = Clean(r.BannerUrl);
        movie.PosterUrl = Clean(r.PosterUrl);
        movie.TrailerUrl = Clean(r.TrailerUrl);
        movie.IsShowing = r.IsShowing;

        // Thay thế danh sách thể loại của phim
        movie.Genres.Clear();

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }
    }

    // 8. Loại bỏ khoảng trắng và xử lý chuỗi rỗng
    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    // 9. Chuyển Entity Movie thành MovieDto
    private static MovieDto ToDto(Movie m) => new()
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
        IsShowing = m.IsShowing,
        Genres = m.Genres
            .OrderBy(g => g.Name)
            .Select(g => new GenreDto
            {
                Id = g.Id,
                Name = g.Name
            })
            .ToList()
    };
}
