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
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly AuthSettings _authSettings;
    private readonly IUserValidator userValidator;

    public AccountService(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IOptions<AuthSettings> authSettings,
        IUserValidator userValidator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _authSettings = authSettings.Value;
        this.userValidator = userValidator;
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        var result = await userValidator.CanRegisterAsync(request);

        if (!result.IsValid)
            throw new BadRequestException(result.Errors);

        var passwordHash = _passwordHasher.Hash(request.Password);

        User user = request.Role switch
        {
            Role.VenueManager => new VenueManager { Email = request.Email, Role = request.Role, PasswordHash = passwordHash },
            Role.ArtistManager => new ArtistManager { Email = request.Email, Role = request.Role, PasswordHash = passwordHash },
            _ => new Customer { Email = request.Email, Role = request.Role, PasswordHash = passwordHash }
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new BadRequestException("Invalid email or password.");

        return await IssueTokensAsync(user);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token is not null)
        {
            token.IsRevoked = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token is null || !token.IsActive)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        // Rotate: revoke old, issue new
        token.IsRevoked = true;
        await _context.SaveChangesAsync();

        return await IssueTokensAsync(token.User);
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null) return null;

        var dto = user.ToDto();
        dto.BaseUrl = RoleRoutes.BaseUrls.TryGetValue(user.Role, out var baseUrl) ? baseUrl : "/";
        return dto;
    }

    public async Task<User?> GetUserEntityByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    private async Task<LoginResponse> IssueTokensAsync(User user)
    {
        var dto = user.ToDto();
        dto.BaseUrl = RoleRoutes.BaseUrls.TryGetValue(user.Role, out var baseUrl) ? baseUrl : "/";

        var accessToken = _tokenService.CreateAccessToken(user.Id, user.Email, user.Role);
        var refreshTokenValue = _tokenService.CreateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            Expires = DateTime.UtcNow.AddDays(_authSettings.RefreshTokenExpirationDays)
        });

        await _context.SaveChangesAsync();

        var expiresInSeconds = _authSettings.AccessTokenExpirationMinutes * 60;
        return new LoginResponse(dto, accessToken, refreshTokenValue, expiresInSeconds);
    }
}
