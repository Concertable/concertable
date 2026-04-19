using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Responses;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using Workers.Functions;
using Xunit;

namespace Concertable.Workers.UnitTests.Functions;

public class ConcertFinishedFunctionTests
{
    private readonly Mock<IConcertRepository> concertRepository;
    private readonly Mock<IFinishedDispatcher> finishedDispatcher;
    private readonly Mock<ILogger<ConcertFinishedFunction>> logger;
    private readonly ConcertFinishedFunction sut;

    public ConcertFinishedFunctionTests()
    {
        concertRepository = new Mock<IConcertRepository>();
        finishedDispatcher = new Mock<IFinishedDispatcher>();
        logger = new Mock<ILogger<ConcertFinishedFunction>>();
        sut = new ConcertFinishedFunction(concertRepository.Object, finishedDispatcher.Object, logger.Object);

        finishedDispatcher.Setup(p => p.FinishedAsync(It.IsAny<int>())).ReturnsAsync(Result.Ok<IFinishOutcome>(new ImmediateFinishOutcome()));
    }

    [Fact]
    public async Task Run_ShouldCallFinishedAsync_ForEachEndedConcert()
    {
        // Arrange
        concertRepository.Setup(r => r.GetEndedConfirmedIdsAsync()).ReturnsAsync([1, 2, 3]);

        // Act
        await sut.Run(null!);

        // Assert
        finishedDispatcher.Verify(p => p.FinishedAsync(1), Times.Once);
        finishedDispatcher.Verify(p => p.FinishedAsync(2), Times.Once);
        finishedDispatcher.Verify(p => p.FinishedAsync(3), Times.Once);
    }

    [Fact]
    public async Task Run_ShouldContinueProcessing_WhenOneFinishFails()
    {
        // Arrange
        concertRepository.Setup(r => r.GetEndedConfirmedIdsAsync()).ReturnsAsync([1, 2, 3]);
        finishedDispatcher.Setup(p => p.FinishedAsync(2)).ReturnsAsync(Result.Fail<IFinishOutcome>("Payment failed"));

        // Act
        await sut.Run(null!);

        // Assert
        finishedDispatcher.Verify(p => p.FinishedAsync(1), Times.Once);
        finishedDispatcher.Verify(p => p.FinishedAsync(2), Times.Once);
        finishedDispatcher.Verify(p => p.FinishedAsync(3), Times.Once);
    }

    [Fact]
    public async Task Run_ShouldNotCallFinishedAsync_WhenNoEndedConcerts()
    {
        // Arrange
        concertRepository.Setup(r => r.GetEndedConfirmedIdsAsync()).ReturnsAsync([]);

        // Act
        await sut.Run(null!);

        // Assert
        finishedDispatcher.Verify(p => p.FinishedAsync(It.IsAny<int>()), Times.Never);
    }
}
