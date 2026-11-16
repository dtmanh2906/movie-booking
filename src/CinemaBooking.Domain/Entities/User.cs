using Microsoft.AspNetCore.Identity;

namespace CinemaBooking.Domain.Entities;

public class User : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;

    public DateTime? BirthDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}