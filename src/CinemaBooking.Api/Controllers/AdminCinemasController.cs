using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CinemaBooking.Application.Cinemas;
using CinemaBooking.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/cinemas")]
public class AdminCinemasController(
    ICinemaService cinemas) : ControllerBase
{
    // 1. Lấy danh sách rạp
    [HttpGet]
    public async Task<ActionResult<List<CinemaDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await cinemas.GetAllAsync(
            cancellationToken);

        return Ok(result);
    }

    // 2. Lấy rạp theo ID
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CinemaDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await cinemas.GetByIdAsync(
            id, cancellationToken);

        return Ok(result);
    }

    // 3. Tạo rạp mới
    [HttpPost]
    public async Task<ActionResult<CinemaDto>> Create(
        CreateCinemaRequest request,
        CancellationToken cancellationToken)
    {
        var created = await cinemas.CreateAsync(
            request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }

    // 4. Cập nhật rạp
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CinemaDto>> Update(
        int id,
        UpdateCinemaRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await cinemas.UpdateAsync(
            id, request, cancellationToken);

        return Ok(updated);
    }

    // 5. Xóa mềm rạp
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        await cinemas.DeleteAsync(
            id, cancellationToken);

        return NoContent();
    }
}