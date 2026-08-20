using Concertable.Kernel.ValueObjects;

namespace Concertable.B2B.Concert.Infrastructure.Services.Settlement;

internal static class SettlementGross
{
    public static Money InGbp(decimal pounds)
    {
        var gross = Money.Gbp(pounds);
        return Money.FromMinorUnits(gross.ToMinorUnits(), gross.Currency);
    }
}
