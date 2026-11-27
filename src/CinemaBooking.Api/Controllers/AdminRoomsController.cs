using CinemaBooking.Application.Rooms;
using CinemaBooking.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/rooms")]
public class AdminRoomsController(IRoomService rooms) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RoomDto>>> GetAll([FromQuery] int? cinemaId, CancellationToken cancellationToken) =>
        Ok(await rooms.GetAllAsync(cinemaId, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await rooms.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<RoomDto>> Create(CreateRoomRequest request, CancellationToken cancellationToken)
    {
        var created = await rooms.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RoomDto>> Update(int id, UpdateRoomRequest request, CancellationToken cancellationToken) =>
        Ok(await rooms.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await rooms.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{roomId:int}/seats")]
    public async Task<ActionResult<List<SeatDto>>> GetSeats(int roomId, CancellationToken cancellationToken) =>
        Ok(await rooms.GetSeatsAsync(roomId, cancellationToken));

    [HttpPut("{roomId:int}/seats/{seatId:int}")]
    public async Task<ActionResult<SeatDto>> UpdateSeat(int roomId, int seatId, UpdateSeatRequest request, CancellationToken cancellationToken) =>
        Ok(await rooms.UpdateSeatAsync(roomId, seatId, request, cancellationToken));
}
