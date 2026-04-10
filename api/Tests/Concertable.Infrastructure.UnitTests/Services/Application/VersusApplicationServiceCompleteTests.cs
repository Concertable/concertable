using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Services.Application;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Application;

public class VersusApplicationServiceCompleteTests
{
    private readonly Mock<IOpportunityApplicationRepository> applicationRepository;
    private readonly Mock<IContractRepository> contractRepository;
    private readonly Mock<IVenueManagerRepository> venueManagerRepository;
    private readonly Mock<IArtistManagerRepository> artistManagerRepository;
    private readonly Mock<IConcertRepository> concertRepository;
    private readonly Mock<IStripeAccountService> stripeAccountService;
    private readonly Mock<IPaymentService> paymentService;
    private readonly Mock<ITransactionService> transactionService;
    private readonly VersusApplicationService sut;

    private readonly OpportunityApplicationEntity application = new() { Id = 5, Status = ApplicationStatus.Settled };
    private readonly VersusContractEntity contract = new() { Guarantee = 200, ArtistDoorPercent = 50 };
    private readonly VenueManagerEntity venueManager = new() { Id = Guid.NewGuid(), Email = "venue@test.com", StripeCustomerId = "cus_venue", Role = Role.VenueManager };
    private readonly ArtistManagerEntity artistManager = new() { Id = Guid.NewGuid(), Email = "artist@test.com", StripeAccountId = "acct_artist", Role = Role.ArtistManager };

    public VersusApplicationServiceCompleteTests()
    {
        applicationRepository = new Mock<IOpportunityApplicationRepository>();
        contractRepository = new Mock<IContractRepository>();
        venueManagerRepository = new Mock<IVenueManagerRepository>();
        artistManagerRepository = new Mock<IArtistManagerRepository>();
        concertRepository = new Mock<IConcertRepository>();
        stripeAccountService = new Mock<IStripeAccountService>();
        paymentService = new Mock<IPaymentService>();
        transactionService = new Mock<ITransactionService>();

        sut = new VersusApplicationService(
            new Mock<IOpportunityApplicationValidator>().Object,
            applicationRepository.Object,
            contractRepository.Object,
            artistManagerRepository.Object,
            venueManagerRepository.Object,
            concertRepository.Object,
            stripeAccountService.Object,
            paymentService.Object,
            new Mock<IConcertService>().Object,
            new Mock<IConcertNotificationService>().Object,
            new Mock<IApplicationNotificationService>().Object,
            transactionService.Object,
            TimeProvider.System);

        applicationRepository.Setup(r => r.GetByConcertIdAsync(10)).ReturnsAsync(application);
        contractRepository.Setup(r => r.GetByApplicationIdAsync<VersusContractEntity>(5)).ReturnsAsync(contract);
        venueManagerRepository.Setup(r => r.GetByApplicationIdAsync(5)).ReturnsAsync(venueManager);
        artistManagerRepository.Setup(r => r.GetByApplicationIdAsync(5)).ReturnsAsync(artistManager);
        stripeAccountService.Setup(s => s.IsUserVerifiedAsync("acct_artist")).ReturnsAsync(true);
        stripeAccountService.Setup(s => s.GetPaymentMethodAsync("cus_venue")).ReturnsAsync("pm_test");
        paymentService.Setup(s => s.ProcessAsync(It.IsAny<TransactionRequest>()))
            .ReturnsAsync(new PaymentResponse { Success = true, Message = "ok", TransactionId = "pi_test_123" });
        concertRepository.Setup(r => r.GetTotalRevenueByConcertIdAsync(10)).ReturnsAsync(1000);
    }

    [Fact]
    public async Task CompleteAsync_ShouldSetStatusToComplete()
    {
        // Act
        await sut.CompleteAsync(10);

        // Assert
        Assert.Equal(ApplicationStatus.Complete, application.Status);
        applicationRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_ShouldProcessCorrectArtistShare()
    {
        // Arrange - guarantee 200 + 50% of 1000 = 700
        TransactionRequest? captured = null;
        paymentService.Setup(s => s.ProcessAsync(It.IsAny<TransactionRequest>()))
            .Callback<TransactionRequest>(r => captured = r)
            .ReturnsAsync(new PaymentResponse { Success = true, Message = "ok", TransactionId = "pi_test_123" });

        // Act
        await sut.CompleteAsync(10);

        // Assert
        Assert.NotNull(captured);
        Assert.Equal(700m, captured.Amount);
        Assert.Equal("pm_test", captured.PaymentMethodId);
        Assert.Equal("acct_artist", captured.DestinationStripeId);
    }

    [Fact]
    public async Task CompleteAsync_ShouldLogTransaction()
    {
        // Act
        await sut.CompleteAsync(10);

        // Assert
        transactionService.Verify(t => t.LogAsync(It.Is<SettlementTransactionDto>(dto =>
            dto.ApplicationId == 5 &&
            dto.FromUserId == venueManager.Id &&
            dto.ToUserId == artistManager.Id &&
            dto.PaymentIntentId == "pi_test_123" &&
            dto.Amount == 70000 &&
            dto.Status == TransactionStatus.Pending
        )), Times.Once);
    }
}
