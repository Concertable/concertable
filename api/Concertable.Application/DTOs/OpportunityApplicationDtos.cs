using Concertable.Artist.Contracts;
using Concertable.Core.Enums;

namespace Concertable.Application.DTOs;

public record OpportunityApplicationDto(int Id, ArtistSummaryDto Artist, OpportunityDto Opportunity, ContractType ContractType, ApplicationStatus Status);
