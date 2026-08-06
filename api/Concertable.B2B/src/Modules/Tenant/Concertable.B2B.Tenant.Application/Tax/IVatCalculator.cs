namespace Concertable.B2B.Tenant.Application.Tax;

internal interface IVatCalculator
{
    decimal Calculate(decimal gross, decimal rate);
}
