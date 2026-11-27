using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CinemaBooking.Application.Showtimes;

// Chỉ dùng cho đầu vào Showtime, không thay đổi cách đọc ngày của API đã có.
public class ExplicitOffsetDateTimeConverter : JsonConverter<DateTimeOffset>
{
    public static bool TryParse(string? text, out DateTimeOffset value)
    {
        value = default;
        if (text is null || !Regex.IsMatch(text, @"T.*(?:Z|[+-]\d{2}:\d{2})$", RegexOptions.CultureInvariant))
            return false;
        // Dùng cùng bộ đọc ISO 8601 của System.Text.Json cho body và query.
        return JsonSerializer.SerializeToElement(text).TryGetDateTimeOffset(out value);
    }

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String || !TryParse(reader.GetString(), out var value))
            throw new JsonException("Thời gian phải theo ISO 8601 và có Z hoặc offset, ví dụ +07:00.");
        return value;
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}
