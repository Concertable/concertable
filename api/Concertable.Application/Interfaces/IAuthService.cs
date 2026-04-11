using Concertable.Application.Requests;
using Concertable.Application.DTOs;

namespace Concertable.Application.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request);
    Task<LoginDto> LoginAsync(LoginRequest request);
    Task LogoutAsync(string refreshToken);
    Task<LoginDto> RefreshTokenAsync(string refreshToken);
    Task SendVerificationEmailAsync(string email);
    Task VerifyEmailAsync(string token);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}
