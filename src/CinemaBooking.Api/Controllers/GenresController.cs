using CinemaBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Api.Controllers;

[ApiController]
[Route("api/genres")]
public class GenresController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public GenresController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var genres = await _dbContext.Genres
            .AsNoTracking()
            .Select(g => new
            {
                g.Id,
                g.Name
            })
            .ToListAsync();

        return Ok(genres);
    }
}
