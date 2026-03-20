using Application.DTOs;
using Application.Interfaces.Concert;
using Application.Mappers;
using Core.Entities;

namespace Infrastructure.Mappers;

public class VersusApplicationMapper : IApplicationMapper
{
    private readonly IConcertOpportunityMapper opportunityMapper;

    public VersusApplicationMapper(IConcertOpportunityMapper opportunityMapper)
        => this.opportunityMapper = opportunityMapper;

    public IConcertApplication ToDto(ConcertApplicationEntity application)
    {
        var entity = (VersusApplicationEntity)application;
        return new VersusApplicationDto(entity.Id, entity.Artist.ToDto(), opportunityMapper.ToDto(entity.Opportunity), entity.Status);
    }

    public IConcertApplication ToArtistDto(ConcertApplicationEntity application)
    {
        var entity = (VersusApplicationEntity)application;
        return new VersusApplicationDto(entity.Id, entity.Artist.ToDto(), opportunityMapper.ToDto(entity.Opportunity), entity.Status);
    }
}
