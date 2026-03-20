using Application.DTOs;
using Application.Interfaces.Concert;
using Application.Mappers;
using Core.Entities;

namespace Infrastructure.Mappers;

public class DoorSplitApplicationMapper : IApplicationMapper
{
    private readonly IConcertOpportunityMapper opportunityMapper;

    public DoorSplitApplicationMapper(IConcertOpportunityMapper opportunityMapper)
        => this.opportunityMapper = opportunityMapper;

    public IConcertApplication ToDto(ConcertApplicationEntity application)
    {
        var entity = (DoorSplitApplicationEntity)application;
        return new DoorSplitApplicationDto(entity.Id, entity.Artist.ToDto(), opportunityMapper.ToDto(entity.Opportunity), entity.Status);
    }

    public IConcertApplication ToArtistDto(ConcertApplicationEntity application)
    {
        var entity = (DoorSplitApplicationEntity)application;
        return new DoorSplitApplicationDto(entity.Id, entity.Artist.ToDto(), opportunityMapper.ToDto(entity.Opportunity), entity.Status);
    }
}
