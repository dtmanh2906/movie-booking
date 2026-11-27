namespace CinemaBooking.Application.Rooms;

public class RoomDto
{
    public int Id { get; set; }
    public int CinemaId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public int SeatsPerRow { get; set; }
    public bool IsActive { get; set; }
}

public class CreateRoomRequest
{
    public int CinemaId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public int SeatsPerRow { get; set; }
}

public class UpdateRoomRequest
{
    public string RoomNumber { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
}