using AutoMapper;
using Core.Entities;
using Core.Entities.Identity;
using Application.Interfaces;
using Core.Parameters;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Tests.Services
{
    [TestFixture]
    public class VenueServiceTests
    {
        private Mock<IVenueRepository> venueRepositoryMock;
        private Mock<IAuthService> authServiceMock;
        private Mock<IMapper> mapperMock;
        private VenueService venueService;

        [SetUp]
        public void Constructor()
        {
            venueRepositoryMock = new Mock<IVenueRepository>();
            authServiceMock = new Mock<IAuthService>();
            mapperMock = new Mock<IMapper>();
            venueService = new VenueService(venueRepositoryMock.Object, authServiceMock.Object, mapperMock.Object);
        }

        [Test]
        public async Task GetHeadersAsync_ShouldReturnHeaders()
        {
            // Arrange
            var venueParams = new SearchParams { Sort = "Name" };
            IEnumerable<Venue> venueHeaders = new List<Venue>
            {
                new Venue { Id = 1, Name = "Venue 1" },
                new Venue { Id = 2, Name = "Venue 2" }
            };
            venueRepositoryMock.Setup(m => m.GetHeadersAsync(venueParams)).ReturnsAsync(venueHeaders);

            // Act
            var result = await venueService.GetHeadersAsync(venueParams);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(venueHeaders.Count(), result.Count());
            Assert.AreEqual(venueHeaders, result);
        }

        [Test]
        public async Task GetUserVenueAsync_ShouldReturnUsersVenue()
        { // Arrange
            var user = new ApplicationUser { Id = 1 };
            var venue = new Venue { Id = 1, Name = "Venue 1", UserId = user.Id };
            authServiceMock.Setup(service => service.GetCurrentUser()).ReturnsAsync(user);
            venueRepositoryMock.Setup(m => m.GetByUserIdAsync(user.Id)).ReturnsAsync(venue);
            // Act
            var result = await venueService.GetUserVenueAsync();
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(venue, result);
        }

        [Test]
        public async Task Create_ShouldAddVenue()
        {
            // Arrange
            var user = new ApplicationUser { Id = 1 };
            var venue = new Venue { Name = "Venue 1" };
            authServiceMock.Setup(service => service.GetCurrentUser()).ReturnsAsync(user);
            // Act
            venueService.Create(venue);
            // Assert
            venueRepositoryMock.Verify(m => m.Add(venue), Times.Once);
            Assert.AreEqual(user.Id, venue.UserId);
        }

        public async Task GetDetailsByIdAsync_ShouldReturnVenueDetails()
        {
            // Arrange
            var venueId = 1;
            var venue = new Venue { Id = 1, Name = "Venue 1" }; 
            venueRepositoryMock.Setup(m => m.GetByIdAsync(venueId)).ReturnsAsync(venue); 
            // Act
            var result = await venueService.GetDetailsByIdAsync(venueId); 
            // Assert
            Assert.NotNull(result); 
            Assert.AreEqual(venue, result); 
        }
    }
}
