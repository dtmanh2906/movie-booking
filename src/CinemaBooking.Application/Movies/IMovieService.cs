using System.Threading.Tasks;
using CinemaBooking.Application.Common;

namespace CinemaBooking.Application.Movies;

public interface IMovieService
{
    // Lấy danh sách phim có phân trang, tìm kiếm
    Task<PagedResult<MovieDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? keyword);

    // Lấy phim theo ID
    Task<MovieDto> GetByIdAsync(int id);

    // Thêm phim
    Task<MovieDto> CreateAsync(
        UpsertMovieRequest request);

    // Cập nhật phim
    Task<MovieDto> UpdateAsync(
        int id,
        UpsertMovieRequest request);

    // Ẩn phim thay vì xóa khỏi database
    Task HideAsync(int id);
}