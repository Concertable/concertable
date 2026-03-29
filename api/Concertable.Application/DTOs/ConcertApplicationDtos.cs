using Concertable.Core.Enums;

namespace Concertable.Application.DTOs;

public record ConcertApplicationDto(int Id, ArtistDto Artist, ConcertOpportunityDto Opportunity, ContractType ContractType, ApplicationStatus Status);
