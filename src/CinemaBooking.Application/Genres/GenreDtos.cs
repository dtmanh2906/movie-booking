namespace CinemaBooking.Application.Genres;

// DTO dùng để trả dữ liệu cho frontend
public class GenreDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

// DTO dùng khi Admin thêm hoặc sửa thể loại
public class UpsertGenreRequest
{
    public string Name { get; set; } = string.Empty;
}