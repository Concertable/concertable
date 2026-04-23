using Concertable.Concert.Application.DTOs;
using Concertable.Concert.Application.Interfaces;

namespace Concertable.Concert.Application.Mappers;

internal class OpportunityMapper : IOpportunityMapper
{
    private readonly IContractMapper contractMapper;

    public OpportunityMapper(IContractMapper contractMapper)
    {
        this.contractMapper = contractMapper;
    }

    public OpportunityDto ToDto(OpportunityEntity opportunity) => new()
    {
        Id = opportunity.Id,
        VenueId = opportunity.VenueId,
        StartDate = opportunity.Period.Start,
        EndDate = opportunity.Period.End,
        Genres = opportunity.OpportunityGenres.Select(og => og.Genre.ToDto()),
        Contract = contractMapper.ToDto(opportunity.Contract)
    };

    public IPagination<OpportunityDto> ToDtos(IPagination<OpportunityEntity> opportunities) =>
        new Pagination<OpportunityDto>(
            opportunities.Data.Select(ToDto),
            opportunities.TotalCount,
            opportunities.PageNumber,
            opportunities.PageSize);
}
