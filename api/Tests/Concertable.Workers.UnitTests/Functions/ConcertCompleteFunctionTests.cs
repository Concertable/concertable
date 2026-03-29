using Concertable.Application.Interfaces.Concert;
using Microsoft.Extensions.Logging;
using Moq;
using Workers.Functions;
using Xunit;

namespace Concertable.Workers.UnitTests.Functions;

public class ConcertCompleteFunctionTests
{
    private readonly Mock<IConcertRepository> concertRepository;
    private readonly Mock<ICompleteProcessor> completeProcessor;
    private readonly Mock<ILogger<ConcertCompleteFunction>> logger;
    private readonly ConcertCompleteFunction sut;

    public ConcertCompleteFunctionTests()
    {
        concertRepository = new Mock<IConcertRepository>();
        completeProcessor = new Mock<ICompleteProcessor>();
        logger = new Mock<ILogger<ConcertCompleteFunction>>();
        sut = new ConcertCompleteFunction(concertRepository.Object, completeProcessor.Object, logger.Object);
    }

    [Fact]
    public async Task Run_ShouldCallCompleteAsync_ForEachEndedConcert()
    {
        // Arrange
        concertRepository.Setup(r => r.GetEndedConfirmedIdsAsync()).ReturnsAsync([1, 2, 3]);

        // Act
        await sut.Run(null!);

        // Assert
        completeProcessor.Verify(p => p.CompleteAsync(1), Times.Once);
        completeProcessor.Verify(p => p.CompleteAsync(2), Times.Once);
        completeProcessor.Verify(p => p.CompleteAsync(3), Times.Once);
    }

    [Fact]
    public async Task Run_ShouldContinueProcessing_WhenOneCompleteFails()
    {
        // Arrange
        concertRepository.Setup(r => r.GetEndedConfirmedIdsAsync()).ReturnsAsync([1, 2, 3]);
        completeProcessor.Setup(p => p.CompleteAsync(2)).ThrowsAsync(new Exception("Payment failed"));

        // Act
        await sut.Run(null!);

        // Assert
        completeProcessor.Verify(p => p.CompleteAsync(1), Times.Once);
        completeProcessor.Verify(p => p.CompleteAsync(2), Times.Once);
        completeProcessor.Verify(p => p.CompleteAsync(3), Times.Once);
    }

    [Fact]
    public async Task Run_ShouldNotCallCompleteAsync_WhenNoEndedConcerts()
    {
        // Arrange
        concertRepository.Setup(r => r.GetEndedConfirmedIdsAsync()).ReturnsAsync([]);

        // Act
        await sut.Run(null!);

        // Assert
        completeProcessor.Verify(p => p.CompleteAsync(It.IsAny<int>()), Times.Never);
    }
}
