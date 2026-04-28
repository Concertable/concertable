using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using Workers.Functions;
using Xunit;

namespace Concertable.Workers.UnitTests.Functions;

public class ConcertFinishedFunctionTests
{
    private readonly Mock<IConcertRepository> concertRepository;
    private readonly Mock<ICompletionExecutor> CompletionExecutor;
    private readonly Mock<ILogger<ConcertFinishedFunction>> logger;
    private readonly ConcertFinishedFunction sut;

    public ConcertFinishedFunctionTests()
    {
        concertRepository = new Mock<IConcertRepository>();
        CompletionExecutor = new Mock<ICompletionExecutor>();
        logger = new Mock<ILogger<ConcertFinishedFunction>>();
        sut = new ConcertFinishedFunction(concertRepository.Object, CompletionExecutor.Object, logger.Object);

        CompletionExecutor.Setup(p => p.FinishAsync(It.IsAny<int>())).ReturnsAsync(Result.Ok<IFinishOutcome>(new ImmediateFinishOutcome()));
    }

    [Fact]
    public async Task Run_ShouldCallFinishAsync_ForEachEndedConcert()
    {
        // Arrange
        concertRepository.Setup(r => r.GetEndedConfirmedIdsAsync()).ReturnsAsync([1, 2, 3]);

        // Act
        await sut.Run(null!);

        // Assert
        CompletionExecutor.Verify(p => p.FinishAsync(1), Times.Once);
        CompletionExecutor.Verify(p => p.FinishAsync(2), Times.Once);
        CompletionExecutor.Verify(p => p.FinishAsync(3), Times.Once);
    }

    [Fact]
    public async Task Run_ShouldContinueProcessing_WhenOneFinishFails()
    {
        // Arrange
        concertRepository.Setup(r => r.GetEndedConfirmedIdsAsync()).ReturnsAsync([1, 2, 3]);
        CompletionExecutor.Setup(p => p.FinishAsync(2)).ReturnsAsync(Result.Fail<IFinishOutcome>("Payment failed"));

        // Act
        await sut.Run(null!);

        // Assert
        CompletionExecutor.Verify(p => p.FinishAsync(1), Times.Once);
        CompletionExecutor.Verify(p => p.FinishAsync(2), Times.Once);
        CompletionExecutor.Verify(p => p.FinishAsync(3), Times.Once);
    }

    [Fact]
    public async Task Run_ShouldNotCallFinishAsync_WhenNoEndedConcerts()
    {
        // Arrange
        concertRepository.Setup(r => r.GetEndedConfirmedIdsAsync()).ReturnsAsync([]);

        // Act
        await sut.Run(null!);

        // Assert
        CompletionExecutor.Verify(p => p.FinishAsync(It.IsAny<int>()), Times.Never);
    }
}
