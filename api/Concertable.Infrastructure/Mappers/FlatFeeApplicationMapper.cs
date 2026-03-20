using Application.DTOs;
using Application.Interfaces.Concert;
using Application.Mappers;
using Core.Entities;

namespace Infrastructure.Mappers;

public class FlatFeeApplicationMapper : IApplicationMapper
{
    private readonly IConcertOpportunityMapper opportunityMapper;

    public FlatFeeApplicationMapper(IConcertOpportunityMapper opportunityMapper)
        => this.opportunityMapper = opportunityMapper;

    public IConcertApplication ToDto(ConcertApplicationEntity application)
    {
        var entity = (FlatFeeApplicationEntity)application;
        return new FlatFeeApplicationDto(entity.Id, entity.Artist.ToDto(), opportunityMapper.ToDto(entity.Opportunity), entity.Status);
    }

    public IConcertApplication ToArtistDto(ConcertApplicationEntity application)
    {
        var entity = (FlatFeeApplicationEntity)application;
        return new FlatFeeApplicationDto(entity.Id, entity.Artist.ToDto(), opportunityMapper.ToDto(entity.Opportunity), entity.Status);
    }
}
