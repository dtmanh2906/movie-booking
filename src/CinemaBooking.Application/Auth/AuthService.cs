using System;
using System.Linq;
using System.Threading.Tasks;
using CinemaBooking.Domain.Constants;
using CinemaBooking.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CinemaBooking.Application.Auth;

public class AuthService(
    UserManager<User> userManager,
    IJwtTokenService jwt) : IAuthService
{
    private const string InvalidCredentials =
        "Email hoặc mật khẩu không đúng.";

    public async Task<AuthResult> RegisterAsync(
        RegisterRequest request)
    {
        var email = request.Email.Trim();

        var user = new User
        {
            UserName = email,
            Email = email,
            FullName = request.FullName.Trim()
        };

        // Identity tự băm mật khẩu trước khi lưu
        var created = await userManager.CreateAsync(
            user, request.Password);

        if (!created.Succeeded)
        {
            return AuthResult.Fail(
                created.Errors.Select(e => e.Description));
        }

        // Người đăng ký mặc định có vai trò Customer
        var roleResult = await userManager.AddToRoleAsync(
            user, Roles.Customer);

        if (!roleResult.Succeeded)
        {
            return AuthResult.Fail(
                roleResult.Errors.Select(e => e.Description));
        }

        return await BuildResultAsync(user);
    }

    public async Task<AuthResult> LoginAsync(
        LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(
            request.Email.Trim());

        if (user is null || !user.IsActive)
        {
            return AuthResult.Fail(
                new[] { InvalidCredentials });
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return AuthResult.Fail(new[]
            {
                "Tài khoản tạm thời bị khóa. Hãy thử lại sau."
            });
        }

        if (!await userManager.CheckPasswordAsync(
                user, request.Password))
        {
            // Ghi nhận lần đăng nhập sai
            await userManager.AccessFailedAsync(user);

            return AuthResult.Fail(
                new[] { InvalidCredentials });
        }

        await userManager.ResetAccessFailedCountAsync(user);

        return await BuildResultAsync(user);
    }

    public async Task<UserDto?> GetUserAsync(int userId)
    {
        var user = await userManager.FindByIdAsync(
            userId.ToString());

        return user is null
            ? null
            : await ToDtoAsync(user);
    }

    private async Task<AuthResult> BuildResultAsync(User user)
    {
        var roles = await userManager.GetRolesAsync(user);

        var (token, expiresAt) =
            jwt.CreateToken(user, roles);

        return AuthResult.Success(new AuthResponse
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            User = await ToDtoAsync(user)
        });
    }

    private async Task<UserDto> ToDtoAsync(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Roles = await userManager.GetRolesAsync(user)
        };
    }
}
