using CinemaBooking.Application.Common;
using CinemaBooking.Application.Exceptions;
using CinemaBooking.Application.PublicCatalog;
using CinemaBooking.Application.Showtimes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/movies")]
public class MoviesController(IPublicCatalogService catalog) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<MovieListItemDto>>> GetMovies(
        [FromQuery] MovieQueryParams queryParams, CancellationToken cancellationToken)
    {
        return Ok(await catalog.GetMoviesAsync(queryParams, cancellationToken));
    }

    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(MovieDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieDetailDto>> GetMovieBySlug(
        string slug, CancellationToken cancellationToken)
    {
        var movie = await catalog.GetMovieBySlugAsync(slug, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy phim đang chiếu.");

        return Ok(movie);
    }

    [HttpGet("{slug}/showtimes")]
    public async Task<ActionResult<List<ShowtimeDto>>> GetShowtimes(
        string slug, [FromQuery] DateOnly? date = null, [FromQuery] int? cinemaId = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await catalog.GetShowtimesAsync(slug, date, cinemaId, cancellationToken));
    }
}
