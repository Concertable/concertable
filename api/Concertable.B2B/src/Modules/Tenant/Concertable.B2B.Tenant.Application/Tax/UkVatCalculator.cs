namespace Concertable.B2B.Tenant.Application.Tax;

internal sealed class UkVatCalculator : IVatCalculator
{
    public decimal Calculate(decimal gross, decimal rate)
        => gross - Math.Round(gross / (1 + rate), 2, MidpointRounding.AwayFromZero);
}
