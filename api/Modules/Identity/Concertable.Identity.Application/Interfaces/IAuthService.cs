using Concertable.Application.Requests;
using Concertable.Identity.Application.DTOs;

namespace Concertable.Application.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task LogoutAsync(string refreshToken);
    Task<LoginResponse> RefreshTokenAsync(string refreshToken);
    Task SendVerificationEmailAsync(string email);
    Task VerifyEmailAsync(string token);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}
