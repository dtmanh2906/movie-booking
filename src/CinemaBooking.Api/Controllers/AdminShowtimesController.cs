using CinemaBooking.Application.Exceptions;
using CinemaBooking.Application.Showtimes;
using CinemaBooking.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/showtimes")]
public class AdminShowtimesController(IShowtimeService showtimes) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ShowtimeDto>>> GetAll([FromQuery] int? movieId, [FromQuery] int? cinemaId,
        [FromQuery] int? roomId, [FromQuery] string? from, [FromQuery] string? to, CancellationToken cancellationToken) =>
        Ok(await showtimes.GetAllAsync(movieId, cinemaId, roomId, ParseTime(from), ParseTime(to), cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ShowtimeDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await showtimes.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ShowtimeDto>> Create(CreateShowtimeRequest request, CancellationToken cancellationToken)
    {
        var created = await showtimes.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ShowtimeDto>> Update(int id, UpdateShowtimeRequest request, CancellationToken cancellationToken) =>
        Ok(await showtimes.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await showtimes.CancelAsync(id, cancellationToken);
        return NoContent();
    }

    // Parse query là xử lý định dạng HTTP; nghiệp vụ nằm trong service.
    private static DateTimeOffset? ParseTime(string? text)
    {
        if (text is null) return null;
        if (!ExplicitOffsetDateTimeConverter.TryParse(text, out var value))
            throw new BadRequestException("Thời gian lọc phải theo ISO 8601 với Z hoặc offset.");
        return value;
    }
}
