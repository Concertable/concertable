using Application.DTOs;
using Application.Requests;

namespace Application.Interfaces;

public interface IAuthService
{
    public Task Register(RegisterRequest request);
    public Task<UserDto> Login(LoginRequest request);
    public Task Logout();
    Task<bool> ConfirmEmailAsync(string userId, string token);
    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request);
    Task RequestEmailChangeAsync(string newEmail);
    Task<bool> ConfirmEmailChangeAsync(string token, string newEmail);
}
