using AutoMapper;
using Core.Entities;
using Core.Entities.Identity;
using Application.Interfaces;
using Core.Parameters;
using Application.DTOs;
using Application.Responses;
using Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Services;

namespace Infrastructure.Tests.Services
{
    [TestFixture]
    public class VenueServiceTests
    {
        private Mock<IVenueRepository> venueRepositoryMock;
        private Mock<IImageService> imageServiceMock;
        private Mock<IReviewService> reviewServiceMock;
        private Mock<ICurrentUserService> currentUserServiceMock;
        private Mock<IGeocodingService> geocodingServiceMock;
        private Mock<IUnitOfWork> unitOfWorkMock;
        private Mock<IMapper> mapperMock;
        private Mock<IGeometryService> geometryServiceMock;
        private VenueService venueService;

        [SetUp]
        public void SetUp()
        {
            venueRepositoryMock = new Mock<IVenueRepository>();
            imageServiceMock = new Mock<IImageService>();
            reviewServiceMock = new Mock<IReviewService>();
            currentUserServiceMock = new Mock<ICurrentUserService>();
            geocodingServiceMock = new Mock<IGeocodingService>();
            unitOfWorkMock = new Mock<IUnitOfWork>();
            mapperMock = new Mock<IMapper>();
            geometryServiceMock = new Mock<IGeometryService>();

            venueService = new VenueService(
                venueRepositoryMock.Object,
                imageServiceMock.Object,
                reviewServiceMock.Object,
                currentUserServiceMock.Object,
                geocodingServiceMock.Object,
                unitOfWorkMock.Object,
                geometryServiceMock.Object,
                mapperMock.Object
            );
        }

        [Test]
        public async Task GetHeadersAsync_ShouldReturnHeadersWithRatings()
        {
            // Arrange
            var venueParams = new SearchParams { Sort = "Name" };

            var venues = new List<Venue>
            {
                new Venue { Id = 1, Name = "Venue 1" },
                new Venue { Id = 2, Name = "Venue 2" }
            };

            var venueHeaders = new List<VenueHeaderDto>
            {
                new VenueHeaderDto { Id = 1, Name = "Venue 1" },
                new VenueHeaderDto { Id = 2, Name = "Venue 2" }
            };

            var paginationResponse = new PaginationResponse<VenueHeaderDto>(
                venueHeaders,
                pageNumber: 1,
                pageSize: 2,
                totalCount: 2
);

            venueRepositoryMock
                .Setup(x => x.GetHeadersAsync(venueParams))
                .ReturnsAsync(paginationResponse);

            mapperMock
                .Setup(x => x.Map<IEnumerable<VenueHeaderDto>>(venues))
                .Returns(venueHeaders);

            reviewServiceMock
                .Setup(x => x.AddAverageRatingsAsync(venueHeaders))
                .Returns(Task.CompletedTask);

            // Act
            var result = await venueService.GetHeadersAsync(venueParams);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Data.Count());
            Assert.AreEqual("Venue 1", result.Data.First().Name);
        }

        [Test]
        public async Task GetDetailsByIdAsync_ShouldReturnMappedDtoWithRating()
        {
            // Arrange
            var venueId = 1;
            var venue = new Venue { Id = venueId, Name = "Venue 1" };
            var venueDto = new VenueDto { Id = venueId, Name = "Venue 1" };

            venueRepositoryMock.Setup(x => x.GetByIdAsync(venueId)).ReturnsAsync(venue);
            mapperMock.Setup(x => x.Map<VenueDto>(venue)).Returns(venueDto);
            reviewServiceMock.Setup(x => x.SetAverageRatingAsync(venueDto)).Returns(Task.CompletedTask);

            // Act
            var result = await venueService.GetDetailsByIdAsync(venueId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(venueDto.Id, result.Id);
        }

        [Test]
        public async Task CreateAsync_ShouldAddVenueAndUpdateUser()
        {
            // Arrange
            var user = new ApplicationUser { Id = 1 };
            var createVenueDto = new CreateVenueDto { Name = "New Venue", Latitude = 1.0, Longitude = 2.0 };
            var venue = new Venue { Name = createVenueDto.Name };
            var createdVenue = new Venue { Id = 1, Name = createVenueDto.Name, UserId = user.Id };
            var venueDto = new VenueDto { Id = 1, Name = "New Venue" };
            var imageMock = new Mock<IFormFile>();

            var venueRepoMock = new Mock<IRepository<Venue>>();
            var userRepoMock = new Mock<IBaseRepository<ApplicationUser>>();

            unitOfWorkMock.Setup(u => u.GetRepository<Venue>()).Returns(venueRepoMock.Object);
            unitOfWorkMock.Setup(u => u.GetBaseRepository<ApplicationUser>()).Returns(userRepoMock.Object);

            currentUserServiceMock.Setup(x => x.GetEntityAsync()).ReturnsAsync(user);
            mapperMock.Setup(x => x.Map<Venue>(createVenueDto)).Returns(venue);
            mapperMock.Setup(x => x.Map<VenueDto>(createdVenue)).Returns(venueDto);
            imageServiceMock.Setup(x => x.UploadAsync(imageMock.Object)).ReturnsAsync("image_url");
            venueRepoMock.Setup(r => r.AddAsync(It.IsAny<Venue>())).ReturnsAsync(createdVenue);
            geocodingServiceMock.Setup(x => x.GetLocationAsync(1.0, 2.0))
                .ReturnsAsync(new LocationDto { County = "County", Town = "Town" });
            geometryServiceMock.Setup(x => x.CreatePoint(1.0, 2.0)).Returns(new NetTopologySuite.Geometries.Point(2.0, 1.0));
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await venueService.CreateAsync(createVenueDto, imageMock.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(venueDto.Name, result.Name);
        }
    }
}
