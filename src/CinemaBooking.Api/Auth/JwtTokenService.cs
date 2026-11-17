using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using CinemaBooking.Application.Auth;
using CinemaBooking.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CinemaBooking.Api.Auth;

public class JwtTokenService(
    IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _jwt = options.Value;

    public (string Token, DateTime ExpiresAt) CreateToken(
        User user,
        IList<string> roles)
    {
        // Chỉ đưa thông tin cần thiết vào token.
        // Không đưa mật khẩu hay PasswordHash vào JWT.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email,
                user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),
            new("name", user.FullName)
        };

        // Thêm vai trò của người dùng
        claims.AddRange(
            roles.Select(role => new Claim("role", role)));

        // Tạo khóa dùng để ký JWT
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwt.Key));

        // Thời gian hết hạn tính theo UTC
        var expires = DateTime.UtcNow
            .AddMinutes(_jwt.ExpiresMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256)
        );

        return (
            new JwtSecurityTokenHandler().WriteToken(token),
            expires
        );
    }
}