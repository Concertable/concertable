using Application.Interfaces.Concert;
using Core.Enums;

namespace Application.DTOs;

public record FlatFeeContractDto : IBookingContract
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ContractType ContractType => ContractType.FlatFee;
    public decimal Fee { get; set; }
}

public record DoorSplitContractDto : IBookingContract
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ContractType ContractType => ContractType.DoorSplit;
    public decimal ArtistDoorPercent { get; set; }
}

public record VersusContractDto : IBookingContract
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ContractType ContractType => ContractType.Versus;
    public decimal Guarantee { get; set; }
    public decimal ArtistDoorPercent { get; set; }
}

public record VenueHireContractDto : IBookingContract
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ContractType ContractType => ContractType.VenueHire;
    public decimal HireFee { get; set; }
}
