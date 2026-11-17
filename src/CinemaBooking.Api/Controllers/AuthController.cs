using System.Threading.Tasks;
using CinemaBooking.Api.Auth;
using CinemaBooking.Application.Auth;
using CinemaBooking.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace CinemaBooking.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService)
    : ControllerBase
{
    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Response);
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request)
    {
        var result = await authService.LoginAsync(request);

        if (!result.Succeeded)
            return Unauthorized(new { errors = result.Errors });

        return Ok(result.Response);
    }

    // GET /api/auth/me
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        // Lấy userId từ JWT đã xác thực
        var userId = User.GetUserId();

        var user = await authService.GetUserAsync(userId);

        if (user is null)
            return NotFound();

        return Ok(user);
    }

    // GET /api/auth/admin-check
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin-check")]
    public IActionResult AdminCheck()
    {
        return Ok(new { message = "Bạn là Admin." });
    }
}
