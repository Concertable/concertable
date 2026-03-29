namespace Concertable.Infrastructure.Calculators;

public static class DoorSplitCalculator
{
    public static decimal ArtistShare(decimal totalRevenue, decimal artistDoorPercent)
        => totalRevenue * (artistDoorPercent / 100);
}
