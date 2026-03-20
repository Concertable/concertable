using Application.DTOs;
using Core.Enums;
using System.Text.Json.Serialization;

namespace Application.Interfaces.Concert;

[JsonDerivedType(typeof(FlatFeeApplicationDto), "flatFee")]
[JsonDerivedType(typeof(DoorSplitApplicationDto), "doorSplit")]
[JsonDerivedType(typeof(VersusApplicationDto), "versus")]
[JsonDerivedType(typeof(VenueHireApplicationDto), "venueHire")]
public interface IConcertApplication
{
    int Id { get; }
    ArtistDto Artist { get; }
    ConcertOpportunityDto Opportunity { get; }
}
