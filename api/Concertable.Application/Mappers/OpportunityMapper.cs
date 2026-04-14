using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces;
using Concertable.Application.Responses;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public class OpportunityMapper : IOpportunityMapper
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
        StartDate = opportunity.StartDate,
        EndDate = opportunity.EndDate,
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
