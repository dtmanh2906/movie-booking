using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Api.Controllers;

public class CreateGenreRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
}