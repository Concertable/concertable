using Concertable.Auth.Infrastructure.Settings;
using Concertable.Identity.Application.Interfaces;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.Options;

namespace Concertable.Auth.Infrastructure.Services;

internal class AuthService : IAuthService
{
    private readonly IAuthUserSeam users;
    private readonly IRefreshTokenRepository refreshTokens;
    private readonly IEmailVerificationTokenRepository emailVerificationTokens;
    private readonly IPasswordResetTokenRepository passwordResetTokens;
    private readonly IPasswordHasher passwordHasher;
    private readonly ITokenService tokenService;
    private readonly AuthSettings authSettings;
    private readonly IEmailService emailService;
    private readonly IAuthUriService authUriService;

    public AuthService(
        IAuthUserSeam users,
        IRefreshTokenRepository refreshTokens,
        IEmailVerificationTokenRepository emailVerificationTokens,
        IPasswordResetTokenRepository passwordResetTokens,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IOptions<AuthSettings> authSettings,
        IEmailService emailService,
        IAuthUriService authUriService)
    {
        this.users = users;
        this.refreshTokens = refreshTokens;
        this.emailVerificationTokens = emailVerificationTokens;
        this.passwordResetTokens = passwordResetTokens;
        this.passwordHasher = passwordHasher;
        this.tokenService = tokenService;
        this.authSettings = authSettings.Value;
        this.emailService = emailService;
        this.authUriService = authUriService;
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        if (request.Role == Role.Admin)
            throw new BadRequestException("You cannot make yourself an admin");

        if (await users.EmailExistsAsync(request.Email))
            throw new BadRequestException("Email already exists");

        var passwordHash = passwordHasher.Hash(request.Password);
        await users.CreateUserAsync(request.Email, passwordHash, request.Role);

        await SendVerificationEmailAsync(request.Email);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var creds = await users.GetCredentialsByEmailAsync(request.Email);

        if (creds is null || !passwordHasher.Verify(request.Password, creds.PasswordHash))
            throw new BadRequestException("Invalid email or password.");

        if (!creds.IsEmailVerified)
            throw new UnauthorizedException("Email not verified.");

        return await IssueTokensAsync(creds);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var token = await refreshTokens.GetByTokenAsync(refreshToken);
        if (token is null) return;

        token.Revoke();
        await refreshTokens.SaveChangesAsync();
    }

    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        var token = await refreshTokens.GetByTokenAsync(refreshToken);

        if (token is null || !token.IsActive)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        token.Revoke();
        await refreshTokens.SaveChangesAsync();

        var creds = await users.GetCredentialsByIdAsync(token.UserId)
            ?? throw new UnauthorizedException("User not found.");

        return await IssueTokensAsync(creds);
    }

    public async Task SendVerificationEmailAsync(string email)
    {
        var creds = await users.GetCredentialsByEmailAsync(email);
        if (creds is null) return;

        var tokenValue = tokenService.CreateRefreshToken();

        await emailVerificationTokens.AddAsync(
            EmailVerificationTokenEntity.Create(creds.Id, tokenValue, DateTime.UtcNow.AddHours(24)));
        await emailVerificationTokens.SaveChangesAsync();

        var link = authUriService.GetEmailVerificationUri(tokenValue);
        await emailService.SendEmailAsync(email, "Verify your email", $"Click the link to verify your email:\n{link}");
    }

    public async Task VerifyEmailAsync(string token)
    {
        var verificationToken = await emailVerificationTokens.GetByTokenAsync(token);

        if (verificationToken is null || !verificationToken.IsActive)
            throw new BadRequestException("Invalid or expired verification token.");

        verificationToken.Use();
        await emailVerificationTokens.SaveChangesAsync();

        await users.SetEmailVerifiedAsync(verificationToken.UserId);
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var creds = await users.GetCredentialsByEmailAsync(email);
        if (creds is null) return;

        var tokenValue = tokenService.CreateRefreshToken();

        await passwordResetTokens.AddAsync(
            PasswordResetTokenEntity.Create(creds.Id, tokenValue, DateTime.UtcNow.AddHours(1)));
        await passwordResetTokens.SaveChangesAsync();

        var link = authUriService.GetPasswordResetUri(tokenValue);
        await emailService.SendEmailAsync(email, "Reset your password", $"Click the link to reset your password:\n{link}");
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var resetToken = await passwordResetTokens.GetByTokenAsync(request.Token);

        if (resetToken is null || !resetToken.IsActive)
            throw new BadRequestException("Invalid or expired password reset token.");

        resetToken.Use();
        await passwordResetTokens.SaveChangesAsync();

        await users.SetPasswordHashAsync(resetToken.UserId, passwordHasher.Hash(request.NewPassword));
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var creds = await users.GetCredentialsByIdAsync(userId);

        if (creds is null || !passwordHasher.Verify(request.CurrentPassword, creds.PasswordHash))
            throw new BadRequestException("Current password is incorrect.");

        await users.SetPasswordHashAsync(userId, passwordHasher.Hash(request.NewPassword));
    }

    private async Task<LoginResponse> IssueTokensAsync(AuthUserCredentials creds)
    {
        var user = await users.GetUserAsync(creds.Id)
            ?? throw new UnauthorizedException("User not found.");

        var accessToken = tokenService.CreateAccessToken(creds.Id, creds.Email, creds.Role);
        var refreshTokenValue = tokenService.CreateRefreshToken();

        await refreshTokens.AddAsync(
            RefreshTokenEntity.Create(creds.Id, refreshTokenValue, DateTime.UtcNow.AddDays(authSettings.RefreshTokenExpirationDays)));
        await refreshTokens.SaveChangesAsync();

        var expiresInSeconds = authSettings.AccessTokenExpirationMinutes * 60;
        return new LoginResponse(user, accessToken, refreshTokenValue, expiresInSeconds);
    }
}
