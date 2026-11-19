
using CinemaBooking.Application.Genres;
using CinemaBooking.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/genres")]
public class AdminGenresController(IGenreService genres) : ControllerBase
{
    // GET: api/admin/genres
    // Lấy tất cả thể loại
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GenreDto>>> GetAll()
    {
        var result = await genres.GetAllAsync();
        return Ok(result);
    }

    // GET: api/admin/genres/1
    // Lấy thể loại theo ID
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GenreDto>> GetById(int id)
    {
        var result = await genres.GetByIdAsync(id);
        return Ok(result);
    }

    // POST: api/admin/genres
    // Thêm thể loại mới
    [HttpPost]
    public async Task<ActionResult<GenreDto>> Create(
        UpsertGenreRequest request)
    {
        var created = await genres.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created
        );
    }

    // PUT: api/admin/genres/1
    // Cập nhật thể loại
    [HttpPut("{id:int}")]
    public async Task<ActionResult<GenreDto>> Update(
        int id,
        UpsertGenreRequest request)
    {
        var updated = await genres.UpdateAsync(id, request);
        return Ok(updated);
    }

    // DELETE: api/admin/genres/1
    // Xóa thể loại
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await genres.DeleteAsync(id);
        return NoContent();
    }
}