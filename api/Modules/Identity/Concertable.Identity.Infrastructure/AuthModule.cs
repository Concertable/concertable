namespace Concertable.Identity.Infrastructure;

internal class AuthModule(
    IAuthService authService,
    IUserService userService,
    IUserMapper userMapper,
    IUserRepository userRepository) : IAuthModule
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

    public async Task<IUser?> GetCurrentUserAsync(Guid userId)
    {
        var entity = await userService.GetUserEntityByIdAsync(userId);
        return entity is null ? null : userMapper.ToDto(entity);
    }

    public async Task<CustomerDto?> GetCustomerAsync(Guid userId)
    {
        var entity = await userService.GetUserEntityByIdAsync(userId);
        if (entity is not CustomerEntity customer) return null;
        return new CustomerDto
        {
            Id = customer.Id,
            Email = customer.Email ?? string.Empty,
            Role = customer.Role,
            StripeCustomerId = customer.StripeCustomerId,
            IsEmailVerified = customer.IsEmailVerified
        };
    }

    public Task<IUser> SaveLocationAsync(Guid userId, double latitude, double longitude) =>
        userService.SaveLocationAsync(latitude, longitude);

    public async Task SetStripeCustomerIdAsync(Guid userId, string stripeCustomerId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null) return;
        user.StripeCustomerId = stripeCustomerId;
        userRepository.Update(user);
        await userRepository.SaveChangesAsync();
    }

}
