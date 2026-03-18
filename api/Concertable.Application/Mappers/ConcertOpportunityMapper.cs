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
        Contract = opportunity.Contract != null
            ? contractMapperFactory.Create(opportunity.Contract.ContractType).ToDto(opportunity.Contract)
            : null
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
        Contract = dto.Contract != null
            ? contractMapperFactory.Create(dto.Contract.ContractType).ToEntity(dto.Contract)
            : null!,
        OpportunityGenres = dto.Genres.Select(g => new OpportunityGenreEntity { GenreId = g.Id }).ToList()
    };
}
