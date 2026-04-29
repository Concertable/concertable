namespace Concertable.Auth.UnitTests.Entities;
public class RefreshTokenEntityTests
{
    [Fact]
    public void IsActive_ShouldReturnTrue_WhenNotRevokedAndNotExpired()
    {
        var token = RefreshTokenEntity.Create(Guid.Empty, "t", DateTime.UtcNow.AddHours(1));

        Assert.True(token.IsActive);
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenRevoked()
    {
        var token = RefreshTokenEntity.Create(Guid.Empty, "t", DateTime.UtcNow.AddHours(1));
        token.Revoke();

        Assert.False(token.IsActive);
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenExpired()
    {
        var token = RefreshTokenEntity.Create(Guid.Empty, "t", DateTime.UtcNow.AddHours(-1));

        Assert.False(token.IsActive);
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenRevokedAndExpired()
    {
        var token = RefreshTokenEntity.Create(Guid.Empty, "t", DateTime.UtcNow.AddHours(-1));
        token.Revoke();

        Assert.False(token.IsActive);
    }
}
