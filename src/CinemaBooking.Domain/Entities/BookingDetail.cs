namespace CinemaBooking.Domain.Entities;

public class BookingDetail
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public int ShowtimeId { get; set; }

    public int SeatId { get; set; }

    public decimal SeatPrice { get; set; }

    public bool IsActive { get; set; } = true;

    public Booking Booking { get; set; } = null!;

    public Showtime Showtime { get; set; } = null!;

    public Seat Seat { get; set; } = null!;
}