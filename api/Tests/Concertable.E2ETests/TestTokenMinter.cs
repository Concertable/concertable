using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Concertable.E2ETests;

internal sealed class TestTokenMinter
{
    private readonly JwtSecurityTokenHandler tokenHandler;
    private readonly SigningCredentials signingCredentials;
    private readonly string issuer;
    private readonly string audience;

    public TestTokenMinter(IConfiguration configuration, JwtSecurityTokenHandler tokenHandler)
    {
        this.tokenHandler = tokenHandler;
        var key = configuration["Auth:TestSigningKey"]
            ?? throw new InvalidOperationException("Auth:TestSigningKey is not configured.");
        issuer = configuration["Auth:TestIssuer"]
            ?? throw new InvalidOperationException("Auth:TestIssuer is not configured.");
        audience = configuration["Auth:TestAudience"]
            ?? throw new InvalidOperationException("Auth:TestAudience is not configured.");

        signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Convert.FromBase64String(key)),
            SecurityAlgorithms.HmacSha256);
    }

    public string Mint(Guid userId, Role role)
    {
        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: [
                new Claim("sub", userId.ToString()),
                new Claim("role", role.ToString())
            ],
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: signingCredentials);

        return tokenHandler.WriteToken(jwt);
    }
}
