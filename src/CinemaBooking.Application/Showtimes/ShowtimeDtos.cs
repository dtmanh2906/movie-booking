using System.Text.Json.Serialization;

namespace CinemaBooking.Application.Showtimes;

public class ShowtimeDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int CinemaId { get; set; }
    public string CinemaName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TicketPrice { get; set; }
    public bool IsCancelled { get; set; }
}

public class CreateShowtimeRequest
{
    public int MovieId { get; set; }
    public int RoomId { get; set; }
    [JsonConverter(typeof(ExplicitOffsetDateTimeConverter))]
    public DateTimeOffset StartTime { get; set; }
    public decimal TicketPrice { get; set; }
}

public class UpdateShowtimeRequest
{
    // Nullable phân biệt trường không gửi với giá trị 0/thời điểm thực tế.
    [JsonConverter(typeof(ExplicitOffsetDateTimeConverter))]
    public DateTimeOffset? StartTime { get; set; }
    public decimal? TicketPrice { get; set; }
}
