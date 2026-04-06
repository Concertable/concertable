using Concertable.Infrastructure.Data;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Repositories;

public class VenueRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext context;
    private readonly VenueRepository sut;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    public VenueRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        context = new ApplicationDbContext(options);
        sut = new VenueRepository(context);

        SeedData();
    }

    private void SeedData()
    {
        context.Users.AddRange(
            new VenueManagerEntity { Id = UserId, Email = "a@test.com", PasswordHash = string.Empty, Role = Role.VenueManager },
            new VenueManagerEntity { Id = OtherUserId, Email = "b@test.com", PasswordHash = string.Empty, Role = Role.VenueManager }
        );

        context.Venues.AddRange(
            new VenueEntity { Id = 1, UserId = UserId, Name = "Venue A", About = "About A", BannerUrl = "a.jpg" },
            new VenueEntity { Id = 2, UserId = OtherUserId, Name = "Venue B", About = "About B", BannerUrl = "b.jpg" }
        );

        context.SaveChanges();
    }

    public void Dispose() => context.Dispose();

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnVenueWithUser_WhenExists()
    {
        var result = await sut.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.NotNull(result.User);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await sut.GetByIdAsync(999);

        Assert.Null(result);
    }

    #endregion

    #region GetByUserIdAsync

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnVenueWithUser_WhenExists()
    {
        var result = await sut.GetByUserIdAsync(UserId);

        Assert.NotNull(result);
        Assert.Equal(UserId, result.UserId);
        Assert.NotNull(result.User);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await sut.GetByUserIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    #endregion

    #region GetIdByUserIdAsync

    [Fact]
    public async Task GetIdByUserIdAsync_ShouldReturnId_WhenExists()
    {
        var result = await sut.GetIdByUserIdAsync(UserId);

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetIdByUserIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await sut.GetIdByUserIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    #endregion
}
