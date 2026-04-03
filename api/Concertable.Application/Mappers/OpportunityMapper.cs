using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Responses;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public class OpportunityMapper : IOpportunityMapper
{
    private readonly IContractMapperFactory contractMapperFactory;

    public OpportunityMapper(IContractMapperFactory contractMapperFactory)
    {
        this.contractMapperFactory = contractMapperFactory;
    }

    public OpportunityDto ToDto(OpportunityEntity opportunity) => new()
    {
        Id = opportunity.Id,
        VenueId = opportunity.VenueId,
        StartDate = opportunity.StartDate,
        EndDate = opportunity.EndDate,
        Genres = opportunity.OpportunityGenres.Select(og => og.Genre.ToDto()),
        Contract = contractMapperFactory.Create(opportunity.Contract.ContractType).ToDto(opportunity.Contract)
    };

    public IPagination<OpportunityDto> ToDtos(IPagination<OpportunityEntity> opportunities) =>
        new Pagination<OpportunityDto>(
            opportunities.Data.Select(ToDto),
            opportunities.TotalCount,
            opportunities.PageNumber,
            opportunities.PageSize);
}
