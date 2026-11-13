using CinemaBooking.Domain.Enums;

namespace CinemaBooking.Domain.Entities;

public class Booking
{
    public int Id { get; set; }

    public string BookingCode { get; set; } = string.Empty;

    public int UserId { get; set; }

    public int ShowtimeId { get; set; }

    public DateTime BookingTime { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public decimal TotalAmount { get; set; }

    public BookingStatus Status { get; set; }
        = BookingStatus.Pending;

    public DateTime? PaidAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public byte[] RowVersion { get; set; }
        = Array.Empty<byte>();

    public User User { get; set; } = null!;

    public Showtime Showtime { get; set; } = null!;

    public ICollection<BookingDetail> BookingDetails { get; set; }
        = new List<BookingDetail>();

    public ICollection<Payment> Payments { get; set; }
        = new List<Payment>();
}