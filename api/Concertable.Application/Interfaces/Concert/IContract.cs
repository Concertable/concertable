using Concertable.Application.DTOs;
using Concertable.Core.Enums;
using System.Text.Json.Serialization;

namespace Concertable.Application.Interfaces.Concert;

[JsonDerivedType(typeof(FlatFeeContractDto), "flatFee")]
[JsonDerivedType(typeof(DoorSplitContractDto), "doorSplit")]
[JsonDerivedType(typeof(VersusContractDto), "versus")]
[JsonDerivedType(typeof(VenueHireContractDto), "venueHire")]
public interface IContract
{
    int Id { get; set; }
    PaymentMethod PaymentMethod { get; set; }
    ContractType ContractType { get; }
}
