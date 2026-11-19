
using CinemaBooking.Application.Common;
using CinemaBooking.Application.Movies;
using CinemaBooking.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/movies")]
public class AdminMoviesController(IMovieService movies) : ControllerBase
{
    // 1. GET: api/admin/movies
    // Lấy danh sách phim có phân trang và tìm kiếm
    [HttpGet]
    public async Task<ActionResult<PagedResult<MovieDto>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null)
    {
        var result = await movies.GetPagedAsync(
            page, pageSize, keyword);

        return Ok(result);
    }

    // 2. GET: api/admin/movies/1
    // Lấy thông tin chi tiết phim
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovieDto>> GetById(int id)
    {
        var result = await movies.GetByIdAsync(id);

        return Ok(result);
    }

    // 3. POST: api/admin/movies
    // Thêm phim mới
    [HttpPost]
    public async Task<ActionResult<MovieDto>> Create(
        UpsertMovieRequest request)
    {
        var created = await movies.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created
        );
    }

    // 4. PUT: api/admin/movies/1
    // Cập nhật thông tin phim
    [HttpPut("{id:int}")]
    public async Task<ActionResult<MovieDto>> Update(
        int id,
        UpsertMovieRequest request)
    {
        var updated = await movies.UpdateAsync(id, request);

        return Ok(updated);
    }

    // 5. DELETE: api/admin/movies/1
    // Ẩn phim thay vì xóa khỏi database
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Hide(int id)
    {
        await movies.HideAsync(id);

        return NoContent();
    }
}
