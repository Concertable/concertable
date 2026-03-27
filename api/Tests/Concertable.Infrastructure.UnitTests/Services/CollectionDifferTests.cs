using Infrastructure.Services;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services;

public class CollectionDifferTests
{
    private readonly CollectionDiffer sut = new();

    [Fact]
    public void GetChanges_ShouldReturnItemsToAdd_WhenNewIdsNotInExisting()
    {
        var (toAdd, toRemove) = sut.GetChanges([1, 2], [1, 2, 3]);

        Assert.Equal([3], toAdd);
        Assert.Empty(toRemove);
    }

    [Fact]
    public void GetChanges_ShouldReturnItemsToRemove_WhenExistingIdsNotInNew()
    {
        var (toAdd, toRemove) = sut.GetChanges([1, 2, 3], [1, 2]);

        Assert.Empty(toAdd);
        Assert.Equal([3], toRemove);
    }

    [Fact]
    public void GetChanges_ShouldReturnBothAddAndRemove_WhenIdsDiffer()
    {
        var (toAdd, toRemove) = sut.GetChanges([1, 2, 3], [2, 3, 4]);

        Assert.Equal([4], toAdd);
        Assert.Equal([1], toRemove);
    }

    [Fact]
    public void GetChanges_ShouldReturnEmpty_WhenCollectionsAreIdentical()
    {
        var (toAdd, toRemove) = sut.GetChanges([1, 2, 3], [1, 2, 3]);

        Assert.Empty(toAdd);
        Assert.Empty(toRemove);
    }

    [Fact]
    public void GetChanges_ShouldReturnAllAsAdd_WhenExistingIsEmpty()
    {
        var (toAdd, toRemove) = sut.GetChanges([], [1, 2, 3]);

        Assert.Equal([1, 2, 3], toAdd);
        Assert.Empty(toRemove);
    }

    [Fact]
    public void GetChanges_ShouldReturnAllAsRemove_WhenNewIsEmpty()
    {
        var (toAdd, toRemove) = sut.GetChanges([1, 2, 3], []);

        Assert.Empty(toAdd);
        Assert.Equal([1, 2, 3], toRemove);
    }

    [Fact]
    public void GetChanges_ShouldDeduplicateIds()
    {
        var (toAdd, toRemove) = sut.GetChanges([1, 1, 2], [2, 3, 3]);

        Assert.Equal([3], toAdd);
        Assert.Equal([1], toRemove);
    }
}
