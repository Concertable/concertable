using Concertable.Application.Interfaces.Concert;
using Microsoft.Extensions.Logging;
using Moq;
using Workers.Functions;
using Xunit;

namespace Concertable.Workers.UnitTests.Functions;

public class ConcertFinishedFunctionTests
{
    private readonly Mock<IConcertRepository> concertRepository;
    private readonly Mock<IFinishedProcessor> finishedProcessor;
    private readonly Mock<ILogger<ConcertFinishedFunction>> logger;
    private readonly ConcertFinishedFunction sut;

    public ConcertFinishedFunctionTests()
    {
        concertRepository = new Mock<IConcertRepository>();
        finishedProcessor = new Mock<IFinishedProcessor>();
        logger = new Mock<ILogger<ConcertFinishedFunction>>();
        sut = new ConcertFinishedFunction(concertRepository.Object, finishedProcessor.Object, logger.Object);
    }

    [Fact]
    public async Task Run_ShouldCallFinishedAsync_ForEachEndedConcert()
    {
        // Arrange
        concertRepository.Setup(r => r.GetEndedConfirmedIdsAsync()).ReturnsAsync([1, 2, 3]);

        // Act
        await sut.Run(null!);

        // Assert
        finishedProcessor.Verify(p => p.FinishedAsync(1), Times.Once);
        finishedProcessor.Verify(p => p.FinishedAsync(2), Times.Once);
        finishedProcessor.Verify(p => p.FinishedAsync(3), Times.Once);
    }

    [Fact]
    public async Task Run_ShouldContinueProcessing_WhenOneFinishFails()
    {
        // Arrange
        concertRepository.Setup(r => r.GetEndedConfirmedIdsAsync()).ReturnsAsync([1, 2, 3]);
        finishedProcessor.Setup(p => p.FinishedAsync(2)).ThrowsAsync(new Exception("Payment failed"));

        // Act
        await sut.Run(null!);

        // Assert
        finishedProcessor.Verify(p => p.FinishedAsync(1), Times.Once);
        finishedProcessor.Verify(p => p.FinishedAsync(2), Times.Once);
        finishedProcessor.Verify(p => p.FinishedAsync(3), Times.Once);
    }

    [Fact]
    public async Task Run_ShouldNotCallFinishedAsync_WhenNoEndedConcerts()
    {
        // Arrange
        concertRepository.Setup(r => r.GetEndedConfirmedIdsAsync()).ReturnsAsync([]);

        // Act
        await sut.Run(null!);

        // Assert
        finishedProcessor.Verify(p => p.FinishedAsync(It.IsAny<int>()), Times.Never);
    }
}
