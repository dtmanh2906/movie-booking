using CinemaBooking.Domain.Enums;

namespace CinemaBooking.Domain.Entities;

public class Payment
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public string PaymentMethod { get; set; }
        = string.Empty;

    public string TxnRef { get; set; }
        = string.Empty;

    public decimal Amount { get; set; }

    public PaymentStatus PaymentStatus { get; set; }
        = PaymentStatus.Pending;

    public string? TransactionId { get; set; }

    public string? ResponseCode { get; set; }

    public string? RawCallback { get; set; }

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime? PaidAt { get; set; }

    public Booking Booking { get; set; } = null!;
}