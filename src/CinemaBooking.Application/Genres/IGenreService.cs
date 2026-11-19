using System.Collections.Generic;
using System.Threading.Tasks;

namespace CinemaBooking.Application.Genres;

public interface IGenreService
{
    // Lấy toàn bộ thể loại
    Task<IReadOnlyList<GenreDto>> GetAllAsync();

    // Lấy thể loại theo ID
    Task<GenreDto> GetByIdAsync(int id);

    // Thêm thể loại
    Task<GenreDto> CreateAsync(UpsertGenreRequest request);

    // Cập nhật thể loại
    Task<GenreDto> UpdateAsync(
        int id,
        UpsertGenreRequest request);

    // Xóa thể loại
    Task DeleteAsync(int id);
}