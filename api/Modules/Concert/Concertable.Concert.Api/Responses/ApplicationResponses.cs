using Concertable.Artist.Contracts;
using Concertable.Contract.Contracts;

namespace Concertable.Concert.Api.Responses;

internal record ApplicationResponse(
    int Id,
    ArtistSummaryDto Artist,
    OpportunitySummaryResponse Opportunity,
    ApplicationStatus Status);

internal record OpportunitySummaryResponse(int Id, DateTime StartDate, DateTime EndDate, IContract Contract);
