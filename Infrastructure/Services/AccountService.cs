using Application.DTOs;
using Application.Interfaces;
using Application.Mappers;
using Application.Requests;
using Application.Responses;
using Core.Entities;
using Core.Enums;
using Core.Exceptions;
using Infrastructure.Constants;
using Infrastructure.Data.Identity;
using Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly ApplicationDbContext context;
    private readonly IPasswordHasher passwordHasher;
    private readonly ITokenService tokenService;
    private readonly AuthSettings authSettings;
    private readonly IUserValidator userValidator;

    public AccountService(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IOptions<AuthSettings> authSettings,
        IUserValidator userValidator)
    {
        this.context = context;
        this.passwordHasher = passwordHasher;
        this.tokenService = tokenService;
        this.authSettings = authSettings.Value;
        this.userValidator = userValidator;
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        var result = await userValidator.CanRegisterAsync(request);

        if (!result.IsValid)
            throw new BadRequestException(result.Errors);

        var passwordHash = passwordHasher.Hash(request.Password);

        User user = request.Role switch
        {
            Role.VenueManager => new VenueManager { Email = request.Email, Role = request.Role, PasswordHash = passwordHash },
            Role.ArtistManager => new ArtistManager { Email = request.Email, Role = request.Role, PasswordHash = passwordHash },
            _ => new Customer { Email = request.Email, Role = request.Role, PasswordHash = passwordHash }
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new BadRequestException("Invalid email or password.");

        return await IssueTokensAsync(user);
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

        return await IssueTokensAsync(token.User);
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null) return null;

        var dto = user.ToDto();
        dto.BaseUrl = RoleRoutes.BaseUrls.TryGetValue(user.Role, out var baseUrl) ? baseUrl : "/";
        return dto;
    }

    public async Task<User?> GetUserEntityByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    private async Task<LoginResponse> IssueTokensAsync(User user)
    {
        var dto = user.ToDto();
        dto.BaseUrl = RoleRoutes.BaseUrls.TryGetValue(user.Role, out var baseUrl) ? baseUrl : "/";

        var accessToken = tokenService.CreateAccessToken(user.Id, user.Email, user.Role);
        var refreshTokenValue = tokenService.CreateRefreshToken();

        context.RefreshTokens.Add(new RefreshToken
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
