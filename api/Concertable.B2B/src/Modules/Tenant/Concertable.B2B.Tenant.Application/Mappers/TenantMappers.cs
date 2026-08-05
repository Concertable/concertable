using Concertable.B2B.Tenant.Application.DTOs;

namespace Concertable.B2B.Tenant.Application.Mappers;

internal static class TenantMappers
{
    public static TenantDto ToDto(this TenantEntity tenant) =>
        new(tenant.Id, tenant.LegalName);

    public static TaxComplianceDto ToDto(this TaxCompliance taxCompliance) => new()
    {
        VatNumber = taxCompliance.VatNumber,
        SellerIdentifier = taxCompliance.SellerIdentifier,
        RegisteredAddress = taxCompliance.RegisteredAddress.ToDto(),
        BankReference = taxCompliance.BankReference,
        HoldsMusicLicence = taxCompliance.HoldsMusicLicence,
    };

    public static RegisteredAddressDto ToDto(this RegisteredAddress address) => new()
    {
        Line1 = address.Line1,
        Line2 = address.Line2,
        City = address.City,
        Postcode = address.Postcode,
        Country = address.Country,
    };

    public static TenantConfigurationDto ToDto(this TenantConfiguration configuration) => new()
    {
        PrsPassThroughRate = configuration.PrsPassThroughRate,
        VatRate = configuration.VatRate,
        PaymentTermsDays = configuration.PaymentTermsDays,
        CancellationNoticeHours = configuration.CancellationNoticeHours,
    };

    public static TaxCompliance ToTaxCompliance(this TaxComplianceDto dto) => new(
        dto.VatNumber,
        dto.SellerIdentifier,
        new RegisteredAddress(
            dto.RegisteredAddress.Line1,
            dto.RegisteredAddress.Line2,
            dto.RegisteredAddress.City,
            dto.RegisteredAddress.Postcode,
            dto.RegisteredAddress.Country),
        dto.BankReference,
        dto.HoldsMusicLicence);

    public static TenantConfiguration ToTenantConfiguration(this TenantConfigurationDto dto) => new(
        dto.PrsPassThroughRate,
        dto.VatRate,
        dto.PaymentTermsDays,
        dto.CancellationNoticeHours);
}
