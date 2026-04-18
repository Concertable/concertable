using Concertable.Application.Interfaces.Search;
using Concertable.Seeding;
using Concertable.Infrastructure.Data;
using Concertable.Core.Entities;
using Concertable.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Repositories;

public class VenueRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext context;
    private readonly Mock<IRatingSpecification<VenueEntity>> ratingSpecification;
    private readonly VenueRepository sut;

    private Guid UserId;
    private Guid OtherUserId;

    public VenueRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        context = new ApplicationDbContext(options);
        ratingSpecification = new Mock<IRatingSpecification<VenueEntity>>();
        sut = new VenueRepository(context, ratingSpecification.Object);

        SeedData();
    }

    private void SeedData()
    {
        var vm1 = VenueManagerEntity.Create("a@test.com", string.Empty);
        var vm2 = VenueManagerEntity.Create("b@test.com", string.Empty);
        context.Users.AddRange(vm1, vm2);
        context.SaveChanges();

        UserId = vm1.Id;
        OtherUserId = vm2.Id;

        context.Venues.AddRange(
            VenueEntity.Create(UserId, "Venue A", "About A", "a.jpg"),
            VenueEntity.Create(OtherUserId, "Venue B", "About B", "b.jpg")
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
