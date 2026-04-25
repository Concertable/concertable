namespace Concertable.Concert.Application.DTOs;

internal record OpportunityApplicationDto(int Id, ArtistSummaryDto Artist, OpportunityDto Opportunity, ApplicationStatus Status);
