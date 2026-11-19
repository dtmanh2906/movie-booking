using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CinemaBooking.Application.Common;

public static class SlugHelper
{
    public static string Slugify(string text)
    {
        // Chuẩn hóa chữ thường và xử lý riêng chữ đ
        var normalized = text.Trim()
            .ToLowerInvariant()
            .Replace('đ', 'd')
            .Normalize(NormalizationForm.FormD);

        var sb = new StringBuilder();

        // Loại bỏ dấu tiếng Việt
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c)
                != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        // Thay các ký tự không hợp lệ bằng dấu -
        var slug = Regex.Replace(
            sb.ToString(),
            "[^a-z0-9]+",
            "-"
        ).Trim('-');

        return slug.Length == 0 ? "phim" : slug;
    }
}