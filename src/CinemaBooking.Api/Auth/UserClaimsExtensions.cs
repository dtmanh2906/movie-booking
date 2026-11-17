using System;
using System.Security.Claims;

namespace CinemaBooking.Api.Auth;

public static class UserClaimsExtensions
{
    // Lấy ID của người dùng từ claim "sub" trong JWT.
    // Chỉ sử dụng sau khi JWT đã được xác thực.

    public static int GetUserId(
        this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst("sub")?.Value;

        return int.TryParse(value, out var id)
            ? id
            : throw new InvalidOperationException(
                "Token không có claim sub hợp lệ.");
    }
}
