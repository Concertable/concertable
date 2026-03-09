using Application.DTOs;
using Application.Interfaces;
using Application.Mappers;
using Application.Requests;
using Application.Responses;
using Core.Entities;
using Core.Exceptions;
using Infrastructure.Constants;
using Infrastructure.Data.Identity;
using Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly AuthSettings _authSettings;

    public AuthService(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IOptions<AuthSettings> authSettings)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _authSettings = authSettings.Value;
    }

    public async Task Register(RegisterRequest request)
    {
        var reasons = new List<string>();

        if (request.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            reasons.Add("You cannot make yourself an admin");

        var validRoles = new[] { "Customer", "VenueManager", "ArtistManager" };
        if (!validRoles.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
            reasons.Add("Invalid role specified");

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            reasons.Add("Email already exists");

        if (reasons.Count != 0)
            throw new BadRequestException(reasons);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user is null)
            throw new BadRequestException("Invalid email or password.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new BadRequestException("Invalid email or password.");

        var dto = user.ToDto();
        dto.BaseUrl = RoleRoutes.BaseUrls.TryGetValue(user.Role, out var baseUrl) ? baseUrl : "/";

        var accessToken = _tokenService.CreateToken(user.Id, user.Email, user.Role);
        var expiresInSeconds = _authSettings.AccessTokenExpirationMinutes * 60;

        return new LoginResponse(dto, accessToken, expiresInSeconds);
    }

    public Task Logout()
    {
        return Task.CompletedTask;
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
            return null;
        var dto = user.ToDto();
        dto.BaseUrl = RoleRoutes.BaseUrls.TryGetValue(user.Role, out var baseUrl) ? baseUrl : "/";
        return dto;
    }

    public async Task<User?> GetUserEntityByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }
}
