namespace CinemaBooking.Application.Cinemas;

public class CinemaDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Hotline { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCinemaRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Hotline { get; set; }
    public string? Email { get; set; }
}

public class UpdateCinemaRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Hotline { get; set; }
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
}