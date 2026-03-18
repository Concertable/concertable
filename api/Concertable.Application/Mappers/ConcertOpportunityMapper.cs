using Application.DTOs;
using Application.Interfaces.Concert;
using Core.Entities;

namespace Application.Mappers;

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
        StartDate = opportunity.StartDate,
        EndDate = opportunity.EndDate,
        Genres = opportunity.OpportunityGenres.Select(og => og.Genre.ToDto()),
        Contract = contractMapperFactory.Create(opportunity.Contract.ContractType).ToDto(opportunity.Contract)
    };

    public OpportunityWithVenueDto ToWithVenueDto(ConcertOpportunityEntity opportunity) => new(
        ToDto(opportunity),
        opportunity.Venue.ToDto()
    );

    public IEnumerable<ConcertOpportunityDto> ToDtos(IEnumerable<ConcertOpportunityEntity> opportunities) =>
        opportunities.Select(ToDto);

    public ConcertOpportunityEntity ToEntity(ConcertOpportunityDto dto) => new()
    {
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        Contract = contractMapperFactory.Create(dto.Contract.ContractType).ToEntity(dto.Contract),
        OpportunityGenres = dto.Genres.Select(g => new OpportunityGenreEntity { GenreId = g.Id }).ToList()
    };
}
