namespace Concertable.Identity.Contracts;

public interface IAuthModule
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
    Task<IUser?> GetCurrentUserAsync(Guid userId);
    Task<CustomerDto?> GetCustomerAsync(Guid userId);
    Task<IUser> SaveLocationAsync(Guid userId, double latitude, double longitude);
    Task SetStripeCustomerIdAsync(Guid userId, string stripeCustomerId);
}
