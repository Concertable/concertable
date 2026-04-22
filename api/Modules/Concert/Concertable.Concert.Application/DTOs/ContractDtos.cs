using Concertable.Concert.Application.Interfaces;

namespace Concertable.Concert.Application.DTOs;

internal record FlatFeeContractDto : IContract
{
    public int Id { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
    public ContractType ContractType => ContractType.FlatFee;
    public decimal Fee { get; set; }
}

internal record DoorSplitContractDto : IContract
{
    public int Id { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
    public ContractType ContractType => ContractType.DoorSplit;
    public decimal ArtistDoorPercent { get; set; }
}

internal record VersusContractDto : IContract
{
    public int Id { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
    public ContractType ContractType => ContractType.Versus;
    public decimal Guarantee { get; set; }
    public decimal ArtistDoorPercent { get; set; }
}

internal record VenueHireContractDto : IContract
{
    public int Id { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
    public ContractType ContractType => ContractType.VenueHire;
    public decimal HireFee { get; set; }
}
