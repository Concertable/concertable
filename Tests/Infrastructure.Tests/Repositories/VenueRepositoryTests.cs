using Core.Entities;
using Core.Enums;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories
{
    [TestFixture]
    public class VenueRepositoryTests : IDisposable
    {
        private ApplicationDbContext context;
        private VenueRepository venueRepository;

        [SetUp]
        public void Constructor()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Concertable")
                .Options;

            context = new ApplicationDbContext(options);
            venueRepository = new VenueRepository(context);
        }

        [Test]
        public async Task GetHeadersAsync_ShouldReturnHeaders()
        {
            // Arrange
            var venueParams = new SearchParams { Sort = "Name", HeaderType = HeaderType.Venue };

            var venueHeaders = new List<Venue> {
                new Venue { Id = 1, Name = "Test Venue 1", About = "About 1", ImageUrl = "", UserId = 1 },
                new Venue { Id = 2, Name = "Test Venue 2", About = "About 2", ImageUrl = "", UserId = 2 }
            };

            // Populate database
            context.Venues.AddRange(venueHeaders);

            // Act
            //var result = await venueRepository.GetHeadersAsync(venueParams);
            // Assert
            //Assert.That(result, Is.Not.Null);
            //Assert.That(result.Count(), Is.EqualTo(venueHeaders.Count()));
        }

        [Test]
        public async Task GetByUserIdAsync_ShouldReturnVenue()
        {
            // Arrange
            int userId = 1;
            // Act
            var result = await venueRepository.GetByUserIdAsync(userId);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.UserId, Is.EqualTo(userId));
            Assert.That(result.Name, Is.EqualTo("Test Venue 1"));
        }

        public void Dispose()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }
    }
}
