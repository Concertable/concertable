using Application.Requests;
using Application.Responses;

namespace Application.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task LogoutAsync(string refreshToken);
    Task<LoginResponse> RefreshTokenAsync(string refreshToken);
}
