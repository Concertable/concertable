namespace Concertable.Auth.Infrastructure;

internal class AuthModule(IAuthService authService) : IAuthModule
{
    public Task RegisterAsync(RegisterRequest request) => authService.RegisterAsync(request);
    public Task<LoginResponse> LoginAsync(LoginRequest request) => authService.LoginAsync(request);
    public Task LogoutAsync(string refreshToken) => authService.LogoutAsync(refreshToken);
    public Task<LoginResponse> RefreshTokenAsync(string refreshToken) => authService.RefreshTokenAsync(refreshToken);
    public Task SendVerificationEmailAsync(string email) => authService.SendVerificationEmailAsync(email);
    public Task VerifyEmailAsync(string token) => authService.VerifyEmailAsync(token);
    public Task ForgotPasswordAsync(string email) => authService.ForgotPasswordAsync(email);
    public Task ResetPasswordAsync(ResetPasswordRequest request) => authService.ResetPasswordAsync(request);
    public Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request) => authService.ChangePasswordAsync(userId, request);
}
