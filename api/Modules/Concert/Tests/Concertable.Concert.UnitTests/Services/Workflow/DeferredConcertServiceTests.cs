using Concertable.Concert.Infrastructure.Services.Workflow;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using FluentResults;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Concertable.Concert.UnitTests.Services.Workflow;

public class DeferredConcertServiceTests
{
    private readonly Mock<IApplicationValidator> applicationValidator;
    private readonly Mock<IManagerPaymentModule> managerPaymentModule;
    private readonly DeferredConcertService sut;

    private readonly Guid payerId = Guid.NewGuid();

    public DeferredConcertServiceTests()
    {
        applicationValidator = new Mock<IApplicationValidator>();
        managerPaymentModule = new Mock<IManagerPaymentModule>();

        applicationValidator
            .Setup(v => v.CanAcceptAsync(It.IsAny<int>()))
            .ReturnsAsync(Result.Ok());

        sut = new DeferredConcertService(
            applicationValidator.Object,
            new Mock<IBookingService>().Object,
            managerPaymentModule.Object,
            new Mock<IConcertDraftService>().Object,
            NullLogger<DeferredConcertService>.Instance);
    }

    [Fact]
    public async Task InitiateAsync_ShouldThrow_WhenVerifyFails()
    {
        // Arrange
        managerPaymentModule
            .Setup(m => m.VerifyAndVoidAsync(payerId, "pm_bad", default))
            .ThrowsAsync(new BadRequestException("Your card was declined."));

        // Act + Assert
        await Assert.ThrowsAsync<BadRequestException>(() => sut.InitiateAsync(1, payerId, "pm_bad"));
    }
}
