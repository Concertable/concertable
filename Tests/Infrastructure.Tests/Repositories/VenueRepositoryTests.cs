using Core.Entities;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            //venueRepository = new VenueRepository(context);
        }

        [Test]
        public async Task GetHeadersAsync_ShouldReturnHeaders()
        { 
            // Arrange
            var venueParams = new SearchParams { Sort = "Name" };

            var venueHeaders = new List<Venue> {
                new Venue { Id = 1, Name = "Test Venue 1", UserId = 1 }, 
                new Venue { Id = 2, Name = "Test Venue 2", UserId = 2 } 
            };

            // Populate database
            context.Venues.AddRange(venueHeaders);

            // Act
            //var result = await venueRepository.GetHeadersAsync(venueParams); 
            // Assert
            //Assert.NotNull(result); 
            //Assert.AreEqual(venueHeaders.Count(), result.Count()); 
            //Assert.AreEqual(venueHeaders, result);
        }

        [Test]
        public async Task GetByUserIdAsync_ShouldReturnVenue()
        { 
            // Arrange
            int userId = 1; 
            // Act
            var result = await venueRepository.GetByUserIdAsync(userId); 
            // Assert
            Assert.NotNull(result); 
            Assert.AreEqual(userId, result.UserId); 
            Assert.AreEqual("Test Venue 1", result.Name);
        }

        public void Dispose()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }
    }
}
