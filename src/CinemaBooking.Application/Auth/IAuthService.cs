using System.Threading.Tasks;

namespace CinemaBooking.Application.Auth;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request);

    Task<AuthResult> LoginAsync(LoginRequest request);

    Task<UserDto?> GetUserAsync(int userId);
}