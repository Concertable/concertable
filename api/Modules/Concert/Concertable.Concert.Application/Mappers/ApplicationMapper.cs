using Concertable.Concert.Application.DTOs;
using Concertable.Concert.Application.Interfaces;

namespace Concertable.Concert.Application.Mappers;

internal class ApplicationMapper : IApplicationMapper
{
    private readonly IOpportunityMapper opportunityMapper;

    public ApplicationMapper(IOpportunityMapper opportunityMapper)
    {
        this.opportunityMapper = opportunityMapper;
    }

    public async Task<ApplicationDto> ToDtoAsync(ApplicationEntity application) =>
        new(application.Id,
            BuildArtistSummary(application),
            await opportunityMapper.ToDtoAsync(application.Opportunity),
            application.Status);

    public async Task<IEnumerable<ApplicationDto>> ToDtosAsync(IEnumerable<ApplicationEntity> applications)
    {
        var applicationList = applications.ToList();
        var opportunityPage = new Pagination<OpportunityEntity>(
            applicationList.Select(a => a.Opportunity), applicationList.Count, 1, applicationList.Count);
        var opportunityDtos = (await opportunityMapper.ToDtosAsync(opportunityPage)).Data
            .ToDictionary(o => o.Id);

        return applicationList.Select(a =>
            new ApplicationDto(
                a.Id,
                BuildArtistSummary(a),
                opportunityDtos[a.Opportunity.Id],
                a.Status));
    }

    private static ArtistSummaryDto BuildArtistSummary(ApplicationEntity application) => new()
    {
        Id = application.Artist.Id,
        Name = application.Artist.Name,
        Avatar = application.Artist.Avatar,
        Genres = application.Artist.Genres.Select(g => new GenreDto(g.Genre.Id, g.Genre.Name))
    };
}
