using Application.Interfaces.Concert;
using Core.Enums;

namespace Application.DTOs;

public record FlatFeeApplicationDto(int Id, ArtistDto Artist, ConcertOpportunityDto Opportunity, ApplicationStatus Status) : IConcertApplication;
public record DoorSplitApplicationDto(int Id, ArtistDto Artist, ConcertOpportunityDto Opportunity, ApplicationStatus Status) : IConcertApplication;
public record VersusApplicationDto(int Id, ArtistDto Artist, ConcertOpportunityDto Opportunity, ApplicationStatus Status) : IConcertApplication;
public record VenueHireApplicationDto(int Id, ArtistDto Artist, ConcertOpportunityDto Opportunity, VenueHireApplicationStatus Status) : IConcertApplication;
