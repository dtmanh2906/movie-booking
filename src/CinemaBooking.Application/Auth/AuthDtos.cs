using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CinemaBooking.Application.Auth;

public class RegisterRequest
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
}

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = new();
}

public class AuthResult
{
    public AuthResponse? Response { get; private init; }

    public IReadOnlyList<string> Errors { get; private init; }
        = Array.Empty<string>();

    public bool Succeeded => Response is not null;

    public static AuthResult Success(AuthResponse response)
        => new() { Response = response };

    public static AuthResult Fail(IEnumerable<string> errors)
        => new() { Errors = errors.ToList() };
}