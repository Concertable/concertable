using System.Text.Json.Serialization;

namespace Concertable.Contract.Abstractions;

[JsonDerivedType(typeof(FlatFeeContract), "flatFee")]
[JsonDerivedType(typeof(DoorSplitContract), "doorSplit")]
[JsonDerivedType(typeof(VersusContract), "versus")]
[JsonDerivedType(typeof(VenueHireContract), "venueHire")]
public interface IContract
{
    int Id { get; set; }
    PaymentMethod PaymentMethod { get; set; }
    ContractType ContractType { get; }
}
