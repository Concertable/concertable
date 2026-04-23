using Concertable.Concert.Application.DTOs;
using System.Text.Json.Serialization;

namespace Concertable.Concert.Application.Interfaces;

[JsonDerivedType(typeof(FlatFeeContractDto), "flatFee")]
[JsonDerivedType(typeof(DoorSplitContractDto), "doorSplit")]
[JsonDerivedType(typeof(VersusContractDto), "versus")]
[JsonDerivedType(typeof(VenueHireContractDto), "venueHire")]
internal interface IContract
{
    int Id { get; set; }
    PaymentMethod PaymentMethod { get; set; }
    ContractType ContractType { get; }
}
