using Concertable.Kernel;

namespace Concertable.B2B.Tenant.Domain.ValueObjects;

public sealed record TenantConfiguration
{
    public decimal? PrsPassThroughRate { get; private init; }
    public decimal? VatRate { get; private init; }
    public int? PaymentTermsDays { get; private init; }
    public int? CancellationNoticeHours { get; private init; }

    private TenantConfiguration() { }

    public TenantConfiguration(
        decimal? prsPassThroughRate,
        decimal? vatRate,
        int? paymentTermsDays,
        int? cancellationNoticeHours)
    {
        ThrowIfRateOutOfRange(prsPassThroughRate, "PRS pass-through rate");
        ThrowIfRateOutOfRange(vatRate, "VAT rate");
        ThrowIfOutOfRange(paymentTermsDays, 0, 365, "Payment terms days");
        ThrowIfOutOfRange(cancellationNoticeHours, 0, 8760, "Cancellation notice hours");

        PrsPassThroughRate = prsPassThroughRate;
        VatRate = vatRate;
        PaymentTermsDays = paymentTermsDays;
        CancellationNoticeHours = cancellationNoticeHours;
    }

    public static TenantConfiguration Empty => new(null, null, null, null);

    private static void ThrowIfRateOutOfRange(decimal? rate, string name)
    {
        if (rate is < 0m or > 1m)
            throw new DomainException($"{name} must be between 0 and 1.");
    }

    private static void ThrowIfOutOfRange(int? value, int minimum, int maximum, string name)
    {
        if (value < minimum || value > maximum)
            throw new DomainException($"{name} must be between {minimum} and {maximum}.");
    }
}
