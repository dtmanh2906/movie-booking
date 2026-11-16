namespace CinemaBooking.Domain.Entities;

public class Showtime
{
    public int Id { get; set; }

    public int MovieId { get; set; }

    public int RoomId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public decimal TicketPrice { get; set; }

    public bool IsCancelled { get; set; }

    public Movie Movie { get; set; } = null!;

    public Room Room { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}