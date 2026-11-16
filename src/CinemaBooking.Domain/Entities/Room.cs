namespace CinemaBooking.Domain.Entities;

public class Room
{
    public int Id { get; set; }

    public int CinemaId { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public int RowCount { get; set; }

    public int SeatsPerRow { get; set; }

    public bool IsActive { get; set; } = true;

    public Cinema Cinema { get; set; } = null!;

    public ICollection<Seat> Seats { get; set; }
        = new List<Seat>();

    public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}