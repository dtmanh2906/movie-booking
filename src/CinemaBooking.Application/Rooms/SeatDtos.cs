using CinemaBooking.Domain.Enums;

namespace CinemaBooking.Application.Rooms;

public class SeatDto
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string SeatRow { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
    public SeatType SeatType { get; set; }
    public decimal ExtraPrice { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateSeatRequest
{
    public SeatType? SeatType { get; set; }
    public decimal? ExtraPrice { get; set; }
}