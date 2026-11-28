
using CinemaBooking.Application.Genres;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.Api.Controllers;

[ApiController]
[Route("api/genres")]
public class GenresController(IGenreService genres) : ControllerBase
{
    // API công khai cho khách hàng xem thể loại phim
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GenreDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await genres.GetAllAsync(cancellationToken);

        return Ok(result);
    }
}
