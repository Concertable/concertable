using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Interfaces;
using Core.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Infrastructure.Settings;

namespace Infrastructure.Services;

public class JwtTokenService : ITokenService
{
    private readonly AuthSettings _settings;

    public JwtTokenService(IOptions<AuthSettings> settings)
    {
        _settings = settings.Value;
    }

    public string CreateAccessToken(int userId, string email, Role role)
    {
        var key = new SymmetricSecurityKey(Convert.FromBase64String(_settings.JwtSigningKeyBase64));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim("sub", userId.ToString()),
            new Claim("email", email),
            new Claim("role", role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
