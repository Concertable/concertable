using Core.Enums;

namespace Application.DTOs;

public record ConcertApplicationDto(int Id, ArtistDto Artist, ConcertOpportunityDto Opportunity, ContractType ContractType, ApplicationStatus Status);
