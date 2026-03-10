using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Interfaces;
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

    public string CreateAccessToken(int userId, string email, string role)
    {
        var key = _settings.JwtSigningKeyBase64 != null
            ? new SymmetricSecurityKey(Convert.FromBase64String(_settings.JwtSigningKeyBase64))
            : new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ConcertableDevSigningKeyAtLeast32Chars!"));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim("sub", userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim("email", email),
            new Claim("role", role)
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
