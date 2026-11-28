using CinemaBooking.Application.Cinemas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/cinemas")]
public class CinemasController(ICinemaService cinemas) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PublicCinemaDto>>> GetPublic(
        [FromQuery] string? city = null, CancellationToken cancellationToken = default)
    {
        return Ok(await cinemas.GetPublicAsync(city, cancellationToken));
    }
}
