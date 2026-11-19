using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinemaBooking.Application.Exceptions;
using CinemaBooking.Application.Genres;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Services;

public class GenreService(AppDbContext db) : IGenreService
{
    // 1. Lấy danh sách thể loại
    public async Task<IReadOnlyList<GenreDto>> GetAllAsync() =>
        await db.Genres
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .Select(g => new GenreDto
            {
                Id = g.Id,
                Name = g.Name
            })
            .ToListAsync();

    // 2. Lấy thể loại theo ID
    public async Task<GenreDto> GetByIdAsync(int id) =>
        ToDto(await FindAsync(id));

    // 3. Thêm thể loại
    public async Task<GenreDto> CreateAsync(
        UpsertGenreRequest request)
    {
        var name = request.Name.Trim();

        // Không cho thêm tên thể loại đã tồn tại
        if (await db.Genres.AnyAsync(g => g.Name == name))
            throw new ConflictException(
                $"Thể loại '{name}' đã tồn tại.");

        var genre = new Genre { Name = name };

        db.Genres.Add(genre);

        await db.SaveChangesAsync();

        return ToDto(genre);
    }

    // 4. Cập nhật thể loại
    public async Task<GenreDto> UpdateAsync(
        int id,
        UpsertGenreRequest request)
    {
        var genre = await FindAsync(id);
        var name = request.Name.Trim();

        // Kiểm tra trùng tên với thể loại khác
        if (await db.Genres.AnyAsync(
            g => g.Name == name && g.Id != id))
        {
            throw new ConflictException(
                $"Thể loại '{name}' đã tồn tại.");
        }

        genre.Name = name;

        await db.SaveChangesAsync();

        return ToDto(genre);
    }

    // 5. Xóa thể loại
    public async Task DeleteAsync(int id)
    {
        var genre = await FindAsync(id);

        // Không xóa thể loại đang được phim sử dụng
        if (await db.Movies.AnyAsync(
            m => m.Genres.Any(g => g.Id == id)))
        {
            throw new ConflictException(
                "Không thể xóa thể loại đang được phim sử dụng.");
        }

        db.Genres.Remove(genre);

        await db.SaveChangesAsync();
    }

    // Hàm dùng chung để tìm thể loại
    private async Task<Genre> FindAsync(int id) =>
        await db.Genres.FirstOrDefaultAsync(g => g.Id == id)
            ?? throw new NotFoundException(
                $"Không tìm thấy thể loại có id {id}.");

    // Chuyển Entity thành DTO
    private static GenreDto ToDto(Genre genre) => new()
    {
        Id = genre.Id,
        Name = genre.Name
    };
}
