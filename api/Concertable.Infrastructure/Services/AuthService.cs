using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Auth;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Concertable.Infrastructure.Data;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Core.Exceptions;
using Concertable.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Concertable.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext context;
    private readonly IPasswordHasher passwordHasher;
    private readonly ITokenService tokenService;
    private readonly AuthSettings authSettings;
    private readonly IUserValidator userValidator;
    private readonly IUserMapper userMapper;
    private readonly IUserLoader userLoader;
    private readonly IEmailService emailService;
    private readonly IAuthUriService authUriService;

    public AuthService(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IOptions<AuthSettings> authSettings,
        IUserValidator userValidator,
        IUserMapper userMapper,
        IUserLoader userLoader,
        IEmailService emailService,
        IAuthUriService authUriService)
    {
        this.context = context;
        this.passwordHasher = passwordHasher;
        this.tokenService = tokenService;
        this.authSettings = authSettings.Value;
        this.userValidator = userValidator;
        this.userMapper = userMapper;
        this.userLoader = userLoader;
        this.emailService = emailService;
        this.authUriService = authUriService;
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        var result = await userValidator.CanRegisterAsync(request);

        if (!result.IsValid)
            throw new BadRequestException(result.Errors);

        var passwordHash = passwordHasher.Hash(request.Password);

        UserEntity user = request.Role switch
        {
            Role.VenueManager => new VenueManagerEntity { Email = request.Email, Role = request.Role, PasswordHash = passwordHash },
            Role.ArtistManager => new ArtistManagerEntity { Email = request.Email, Role = request.Role, PasswordHash = passwordHash },
            Role.Admin => new AdminEntity { Email = request.Email, Role = request.Role, PasswordHash = passwordHash },
            _ => new CustomerEntity { Email = request.Email, Role = request.Role, PasswordHash = passwordHash }
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        await SendVerificationEmailAsync(request.Email);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new BadRequestException("Invalid email or password.");

        if (!user.IsEmailVerified)
            throw new UnauthorizedException("Email not verified.");

        var fullUser = await userLoader.LoadAsync(user);

        return await IssueTokensAsync(fullUser);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token is not null)
        {
            token.IsRevoked = true;
            await context.SaveChangesAsync();
        }
    }

    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        var token = await context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token is null || !token.IsActive)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        token.IsRevoked = true;
        await context.SaveChangesAsync();

        var user = await userLoader.LoadAsync(token.User);

        return await IssueTokensAsync(user);
    }

    public async Task SendVerificationEmailAsync(string email)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
            return;

        var tokenValue = tokenService.CreateRefreshToken();

        context.EmailVerificationTokens.Add(new EmailVerificationTokenEntity
        {
            UserId = user.Id,
            Token = tokenValue,
            Expires = DateTime.UtcNow.AddHours(24)
        });

        await context.SaveChangesAsync();

        var link = authUriService.GetEmailVerificationUri(tokenValue);
        await emailService.SendEmailAsync(email, "Verify your email", $"Click the link to verify your email:\n{link}");
    }

    public async Task VerifyEmailAsync(string token)
    {
        var verificationToken = await context.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token);

        if (verificationToken is null || !verificationToken.IsActive)
            throw new BadRequestException("Invalid or expired verification token.");

        verificationToken.IsUsed = true;
        verificationToken.User.IsEmailVerified = true;

        await context.SaveChangesAsync();
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
            return;

        var tokenValue = tokenService.CreateRefreshToken();

        context.PasswordResetTokens.Add(new PasswordResetTokenEntity
        {
            UserId = user.Id,
            Token = tokenValue,
            Expires = DateTime.UtcNow.AddHours(1)
        });

        await context.SaveChangesAsync();

        var link = authUriService.GetPasswordResetUri(tokenValue);
        await emailService.SendEmailAsync(email, "Reset your password", $"Click the link to reset your password:\n{link}");
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var resetToken = await context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.Token);

        if (resetToken is null || !resetToken.IsActive)
            throw new BadRequestException("Invalid or expired password reset token.");

        resetToken.User.PasswordHash = passwordHasher.Hash(request.NewPassword);
        resetToken.IsUsed = true;

        await context.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null || !passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new BadRequestException("Current password is incorrect.");

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        await context.SaveChangesAsync();
    }

    private async Task<LoginResponse> IssueTokensAsync(UserEntity user)
    {
        var dto = userMapper.ToDto(user);
        var accessToken = tokenService.CreateAccessToken(user.Id, user.Email, user.Role);
        var refreshTokenValue = tokenService.CreateRefreshToken();

        context.RefreshTokens.Add(new RefreshTokenEntity
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            Expires = DateTime.UtcNow.AddDays(authSettings.RefreshTokenExpirationDays)
        });

        await context.SaveChangesAsync();

        var expiresInSeconds = authSettings.AccessTokenExpirationMinutes * 60;
        return new LoginResponse(dto, accessToken, refreshTokenValue, expiresInSeconds);
    }
}
