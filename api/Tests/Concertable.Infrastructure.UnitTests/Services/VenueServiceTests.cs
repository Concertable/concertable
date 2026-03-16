//using Core.Entities;
//using Application.Interfaces;
//using Application.DTOs;
//using Application.Requests;
//using Core.Exceptions;
//using Microsoft.AspNetCore.Http;
//using Moq;
//using NUnit.Framework;
//using Infrastructure.Services;

//namespace Infrastructure.Tests.Services;

//[TestFixture]
//public class VenueServiceTests
//{
//    private Mock<IVenueRepository> venueRepositoryMock;
//    private Mock<IImageService> imageServiceMock;
//    private Mock<IReviewService> reviewServiceMock;
//    private Mock<ICurrentUser> currentUserMock;
//    private Mock<IGeocodingService> geocodingServiceMock;
//    private Mock<IUnitOfWork> unitOfWorkMock;
//    private Mock<IGeometryProvider> geometryServiceMock;
//    private VenueService venueService;

//    [SetUp]
//    public void SetUp()
//    {
//        venueRepositoryMock = new Mock<IVenueRepository>();
//        imageServiceMock = new Mock<IImageService>();
//        reviewServiceMock = new Mock<IReviewService>();
//        currentUserMock = new Mock<ICurrentUser>();
//        geocodingServiceMock = new Mock<IGeocodingService>();
//        unitOfWorkMock = new Mock<IUnitOfWork>();
//        geometryServiceMock = new Mock<IGeometryProvider>();

//        venueService = new VenueService(
//            venueRepositoryMock.Object,
//            imageServiceMock.Object,
//            reviewServiceMock.Object,
//            currentUserMock.Object,
//            geocodingServiceMock.Object,
//            unitOfWorkMock.Object,
//            geometryServiceMock.Object
//        );
//    }

//    [Test]
//    public async Task GetDetailsByIdAsync_ShouldReturnMappedDtoWithRating()
//    {
//        // Arrange
//        var venueId = 1;
//        var venue = new VenueEntity { Id = venueId, Name = "Venue 1", About = "About", ImageUrl = "" };

//        venueRepositoryMock.Setup(x => x.GetByIdAsync(venueId)).ReturnsAsync(venue);
//        reviewServiceMock.Setup(x => x.SetAverageRatingAsync(It.IsAny<VenueDto>())).Returns(Task.CompletedTask);

//        // Act
//        var result = await venueService.GetDetailsByIdAsync(venueId);

//        // Assert
//        Assert.That(result, Is.Not.Null);
//        Assert.That(result.Id, Is.EqualTo(venueId));
//    }

//    [Test]
//    public async Task CreateAsync_ShouldAddVenueAndUpdateUser()
//    {
//        // Arrange
//        var user = new ApplicationUser { Id = 1 };
//        var createVenueRequest = new CreateVenueRequest("New Venue", "About the venue", 1.0, 2.0);
//        var createdVenue = new VenueEntity { Id = 1, Name = "New Venue", About = "About the venue", ImageUrl = "", UserId = user.Id };
//        var imageMock = new Mock<IFormFile>();

//        var venueRepoMock = new Mock<IRepository<VenueEntity>>();
//        var userRepoMock = new Mock<IBaseRepository<ApplicationUser>>();

//        unitOfWorkMock.Setup(u => u.GetRepository<VenueEntity>()).Returns(venueRepoMock.Object);
//        unitOfWorkMock.Setup(u => u.GetBaseRepository<ApplicationUser>()).Returns(userRepoMock.Object);

//        currentUserMock.Setup(x => x.GetEntity()).Returns(user);
//        imageServiceMock.Setup(x => x.UploadAsync(imageMock.Object)).ReturnsAsync("image_url");
//        venueRepoMock.Setup(r => r.AddAsync(It.IsAny<VenueEntity>())).ReturnsAsync(createdVenue);
//        geocodingServiceMock.Setup(x => x.GetLocationAsync(1.0, 2.0))
//            .ReturnsAsync(new LocationDto("County", "Town"));
//        geometryServiceMock.Setup(x => x.CreatePoint(1.0, 2.0)).Returns(new NetTopologySuite.Geometries.Point(2.0, 1.0));
//        unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

//        // Act
//        var result = await venueService.CreateAsync(createVenueRequest, imageMock.Object);

//        // Assert
//        Assert.That(result, Is.Not.Null);
//        Assert.That(result.Name, Is.EqualTo("New Venue"));
//    }
//}
