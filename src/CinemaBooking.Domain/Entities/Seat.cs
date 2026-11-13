using CinemaBooking.Domain.Enums;

namespace CinemaBooking.Domain.Entities;

public class Seat
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public string SeatRow { get; set; } = string.Empty;

    public int SeatNumber { get; set; }

    public SeatType SeatType { get; set; } = SeatType.Standard;

    public decimal ExtraPrice { get; set; }

    public bool IsActive { get; set; } = true;

    public Room Room { get; set; } = null!;

    public ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
}