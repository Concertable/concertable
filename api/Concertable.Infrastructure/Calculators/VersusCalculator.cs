namespace Concertable.Infrastructure.Calculators;

public static class VersusCalculator
{
    public static decimal ArtistShare(decimal guarantee, decimal totalRevenue, decimal artistDoorPercent)
        => guarantee + (totalRevenue * (artistDoorPercent / 100));
}
