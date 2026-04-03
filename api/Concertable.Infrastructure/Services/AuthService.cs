using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Auth;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Concertable.Infrastructure.Data;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Core.Exceptions;
using Concertable.Infrastructure.Constants;
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

    public AuthService(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IOptions<AuthSettings> authSettings,
        IUserValidator userValidator,
        IUserMapper userMapper)
    {
        this.context = context;
        this.passwordHasher = passwordHasher;
        this.tokenService = tokenService;
        this.authSettings = authSettings.Value;
        this.userValidator = userValidator;
        this.userMapper = userMapper;
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
            _ => new CustomerEntity { Email = request.Email, Role = request.Role, PasswordHash = passwordHash }
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await context.Users
            .Include(u => (u as VenueManagerEntity)!.Venue)
            .Include(u => (u as ArtistManagerEntity)!.Artist)
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

        var user = await context.Users
            .Include(u => (u as VenueManagerEntity)!.Venue)
            .Include(u => (u as ArtistManagerEntity)!.Artist)
            .FirstAsync(u => u.Id == token.User.Id);

        return await IssueTokensAsync(user);
    }

    private async Task<LoginResponse> IssueTokensAsync(UserEntity user)
    {
        var dto = userMapper.ToDto(user);
        var baseUrl = RoleRoutes.BaseUrls.TryGetValue(user.Role, out var url) ? url : "/";

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
        return new LoginResponse(dto, accessToken, refreshTokenValue, expiresInSeconds, baseUrl);
    }
}
