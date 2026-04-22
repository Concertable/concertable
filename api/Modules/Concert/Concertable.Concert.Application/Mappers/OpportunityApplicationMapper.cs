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

    public OpportunityApplicationDto ToDto(OpportunityApplicationEntity application) =>
        new(application.Id,
            new ArtistSummaryDto
            {
                Id = application.Artist.Id,
                Name = application.Artist.Name,
                Avatar = application.Artist.Avatar,
                Genres = application.Artist.Genres.Select(g => new GenreDto(g.Genre.Id, g.Genre.Name))
            },
            opportunityMapper.ToDto(application.Opportunity),
            application.Opportunity.Contract.ContractType,
            application.Status);

    public IEnumerable<OpportunityApplicationDto> ToDtos(IEnumerable<OpportunityApplicationEntity> applications) =>
        applications.Select(ToDto);
}
