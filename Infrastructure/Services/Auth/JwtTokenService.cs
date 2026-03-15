using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Interfaces.Auth;
using Core.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Infrastructure.Settings;

namespace Infrastructure.Services.Auth;

public class JwtTokenService : ITokenService
{
    private readonly AuthSettings settings;

    public JwtTokenService(IOptions<AuthSettings> settings)
    {
        this.settings = settings.Value;
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

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(settings.AccessTokenExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
