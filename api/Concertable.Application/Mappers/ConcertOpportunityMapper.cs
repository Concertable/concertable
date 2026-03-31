using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Responses;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public class ConcertOpportunityMapper : IConcertOpportunityMapper
{
    private readonly IContractMapperFactory contractMapperFactory;

    public ConcertOpportunityMapper(IContractMapperFactory contractMapperFactory)
    {
        this.contractMapperFactory = contractMapperFactory;
    }

    public ConcertOpportunityDto ToDto(ConcertOpportunityEntity opportunity) => new()
    {
        Id = opportunity.Id,
        VenueId = opportunity.VenueId,
        StartDate = opportunity.StartDate,
        EndDate = opportunity.EndDate,
        Genres = opportunity.OpportunityGenres.Select(og => og.Genre.ToDto()),
        Contract = contractMapperFactory.Create(opportunity.Contract.ContractType).ToDto(opportunity.Contract)
    };

    public IPagination<ConcertOpportunityDto> ToDtos(IPagination<ConcertOpportunityEntity> opportunities) =>
        new Pagination<ConcertOpportunityDto>(
            opportunities.Data.Select(ToDto),
            opportunities.TotalCount,
            opportunities.PageNumber,
            opportunities.PageSize);
}
