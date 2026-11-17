using CinemaBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaBooking.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using CinemaBooking.Domain.Entities;
using System.ComponentModel.DataAnnotations;

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

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create(
    [FromBody] CreateGenreRequest request)
    {
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new
            {
                message = "Tên thể loại không được để trống."
            });
        }

        var genre = new Genre
        {
            Name = name
        };

        _dbContext.Genres.Add(genre);

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            genre.Id,
            genre.Name
        });
    }
}
