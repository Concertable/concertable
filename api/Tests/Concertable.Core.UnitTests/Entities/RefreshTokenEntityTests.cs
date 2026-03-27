using Core.Entities;
using Xunit;

namespace Concertable.Core.UnitTests.Entities;

public class RefreshTokenEntityTests
{
    [Fact]
    public void IsActive_ShouldReturnTrue_WhenNotRevokedAndNotExpired()
    {
        var token = new RefreshTokenEntity { Token = "t", Expires = DateTime.UtcNow.AddHours(1) };

        Assert.True(token.IsActive);
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenRevoked()
    {
        var token = new RefreshTokenEntity { Token = "t", Expires = DateTime.UtcNow.AddHours(1), IsRevoked = true };

        Assert.False(token.IsActive);
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenExpired()
    {
        var token = new RefreshTokenEntity { Token = "t", Expires = DateTime.UtcNow.AddHours(-1) };

        Assert.False(token.IsActive);
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenRevokedAndExpired()
    {
        var token = new RefreshTokenEntity { Token = "t", Expires = DateTime.UtcNow.AddHours(-1), IsRevoked = true };

        Assert.False(token.IsActive);
    }
}
