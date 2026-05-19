namespace Concertable.Concert.Contracts;

public record VenueDashboardCountsDto(
    int ApplicationsAwaitingReview,
    int OpenOpportunities,
    int UpcomingConcerts);
