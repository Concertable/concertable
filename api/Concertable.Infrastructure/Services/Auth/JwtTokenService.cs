using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Application.Interfaces.Auth;
using Core.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Infrastructure.Settings;

namespace Infrastructure.Services.Auth;

public class JwtTokenService : ITokenService
{
    private readonly AuthSettings settings;
    private readonly TimeProvider timeProvider;
    private readonly JwtSecurityTokenHandler tokenHandler;
    private readonly RandomNumberGenerator rng;

    public JwtTokenService(
        IOptions<AuthSettings> settings,
        TimeProvider timeProvider,
        JwtSecurityTokenHandler tokenHandler,
        RandomNumberGenerator rng)
    {
        this.settings = settings.Value;
        this.timeProvider = timeProvider;
        this.tokenHandler = tokenHandler;
        this.rng = rng;
    }

    public string CreateAccessToken(int userId, string email, Role role)
    {
        var key = new SymmetricSecurityKey(Convert.FromBase64String(settings.JwtSigningKeyBase64));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("sub", userId.ToString()),
            new Claim("email", email),
            new Claim("role", role.ToString())
        };

        var now = timeProvider.GetUtcNow().UtcDateTime;

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(settings.AccessTokenExpirationMinutes),
            signingCredentials: creds);

        return tokenHandler.WriteToken(token);
    }

    public string CreateRefreshToken()
    {
        var bytes = new byte[64];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}