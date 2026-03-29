using Concertable.Application.Requests;
using Concertable.Application.Responses;

namespace Concertable.Application.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task LogoutAsync(string refreshToken);
    Task<LoginResponse> RefreshTokenAsync(string refreshToken);
}
