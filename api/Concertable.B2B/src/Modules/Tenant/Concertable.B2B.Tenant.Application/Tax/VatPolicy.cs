namespace Concertable.B2B.Tenant.Application.Tax;

internal sealed class VatPolicy : IVatPolicy
{
    private readonly IVatCalculator calculator;

    public VatPolicy(IVatCalculator calculator)
    {
        this.calculator = calculator;
    }

    public VatCalculation Apply(decimal gross, string? supplierVatNumber, decimal rate)
    {
        if (string.IsNullOrWhiteSpace(supplierVatNumber)) return VatCalculation.None(gross);
        var vat = calculator.Calculate(gross, rate);
        return new VatCalculation(gross - vat, vat, rate);
    }
}
