using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CinemaBooking.Infrastructure.Persistence;

public class UtcDateTimeConverter
    : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() : base(
        // Khi ghi database: đổi giờ Local sang UTC.
        v => v.Kind == DateTimeKind.Local
            ? v.ToUniversalTime()
            : v,

        // Khi đọc database: đánh dấu dữ liệu là UTC.
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}

public class NullableUtcDateTimeConverter
    : ValueConverter<DateTime?, DateTime?>
{
    public NullableUtcDateTimeConverter() : base(
        v => v.HasValue && v.Value.Kind == DateTimeKind.Local
            ? v.Value.ToUniversalTime()
            : v,

        v => v.HasValue
            ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
            : v)
    {
    }
}