using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using Workers.Functions;
using Xunit;

namespace Concertable.Workers.UnitTests.Functions;
public class ConcertFinishedFunctionTests
{
    private readonly Mock<IConcertRepository> concertRepository;
    private readonly Mock<ICompletionDispatcher> CompletionDispatcher;
    private readonly Mock<ILogger<ConcertFinishedFunction>> logger;
    private readonly ConcertFinishedFunction sut;

    public ConcertFinishedFunctionTests()
    {
        concertRepository = new Mock<IConcertRepository>();
        CompletionDispatcher = new Mock<ICompletionDispatcher>();
        logger = new Mock<ILogger<ConcertFinishedFunction>>();
        sut = new ConcertFinishedFunction(concertRepository.Object, CompletionDispatcher.Object, logger.Object);

        CompletionDispatcher.Setup(p => p.FinishAsync(It.IsAny<int>())).ReturnsAsync(Result.Ok());
    }

    [Fact]
    public async Task Run_ShouldCallFinishAsync_ForEachEndedConcert()
    {
        // Arrange
        concertRepository.Setup(r => r.GetEndedConfirmedIdsAsync()).ReturnsAsync([1, 2, 3]);

        // Act
        await sut.Run(null!);

        // Assert
        CompletionDispatcher.Verify(p => p.FinishAsync(1), Times.Once);
        CompletionDispatcher.Verify(p => p.FinishAsync(2), Times.Once);
        CompletionDispatcher.Verify(p => p.FinishAsync(3), Times.Once);
    }

    [Fact]
    public async Task Run_ShouldContinueProcessing_WhenOneFinishFails()
    {
        // Arrange
        concertRepository.Setup(r => r.GetEndedConfirmedIdsAsync()).ReturnsAsync([1, 2, 3]);
        CompletionDispatcher.Setup(p => p.FinishAsync(2)).ReturnsAsync(Result.Fail("Payment failed"));

        // Act
        await sut.Run(null!);

        // Assert
        CompletionDispatcher.Verify(p => p.FinishAsync(1), Times.Once);
        CompletionDispatcher.Verify(p => p.FinishAsync(2), Times.Once);
        CompletionDispatcher.Verify(p => p.FinishAsync(3), Times.Once);
    }

    [Fact]
    public async Task Run_ShouldNotCallFinishAsync_WhenNoEndedConcerts()
    {
        // Arrange
        concertRepository.Setup(r => r.GetEndedConfirmedIdsAsync()).ReturnsAsync([]);

        // Act
        await sut.Run(null!);

        // Assert
        CompletionDispatcher.Verify(p => p.FinishAsync(It.IsAny<int>()), Times.Never);
    }
}
