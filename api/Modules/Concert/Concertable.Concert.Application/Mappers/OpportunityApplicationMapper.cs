using Concertable.Concert.Application.DTOs;
using Concertable.Concert.Application.Interfaces;

namespace Concertable.Concert.Application.Mappers;

internal class OpportunityApplicationMapper : IOpportunityApplicationMapper
{
    private readonly IOpportunityMapper opportunityMapper;

    public OpportunityApplicationMapper(IOpportunityMapper opportunityMapper)
    {
        this.opportunityMapper = opportunityMapper;
    }

    public async Task<OpportunityApplicationDto> ToDtoAsync(OpportunityApplicationEntity application) =>
        new(application.Id,
            BuildArtistSummary(application),
            await opportunityMapper.ToDtoAsync(application.Opportunity),
            application.Status);

    public async Task<IEnumerable<OpportunityApplicationDto>> ToDtosAsync(IEnumerable<OpportunityApplicationEntity> applications)
    {
        var applicationList = applications.ToList();
        var opportunityPage = new Pagination<OpportunityEntity>(
            applicationList.Select(a => a.Opportunity), applicationList.Count, 1, applicationList.Count);
        var opportunityDtos = (await opportunityMapper.ToDtosAsync(opportunityPage)).Data
            .ToDictionary(o => o.Id);

        return applicationList.Select(a =>
            new OpportunityApplicationDto(
                a.Id,
                BuildArtistSummary(a),
                opportunityDtos[a.Opportunity.Id],
                a.Status));
    }

    private static ArtistSummaryDto BuildArtistSummary(OpportunityApplicationEntity application) => new()
    {
        Id = application.Artist.Id,
        Name = application.Artist.Name,
        Avatar = application.Artist.Avatar,
        Genres = application.Artist.Genres.Select(g => new GenreDto(g.Genre.Id, g.Genre.Name))
    };
}
