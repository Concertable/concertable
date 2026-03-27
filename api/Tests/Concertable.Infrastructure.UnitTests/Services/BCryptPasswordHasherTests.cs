using Infrastructure.Services.Auth;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher sut = new();

    [Fact]
    public void Hash_ShouldReturnHashedPassword()
    {
        var result = sut.Hash("password123");

        Assert.NotNull(result);
        Assert.NotEqual("password123", result);
    }

    [Fact]
    public void Hash_ShouldReturnDifferentHashEachTime()
    {
        var hash1 = sut.Hash("password123");
        var hash2 = sut.Hash("password123");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Verify_ShouldReturnTrue_WhenPasswordMatchesHash()
    {
        var hash = sut.Hash("password123");

        Assert.True(sut.Verify("password123", hash));
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        var hash = sut.Hash("password123");

        Assert.False(sut.Verify("wrongpassword", hash));
    }
}
