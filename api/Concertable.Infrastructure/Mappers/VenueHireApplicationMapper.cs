using Application.DTOs;
using Application.Interfaces.Concert;
using Application.Mappers;
using Core.Entities;

namespace Infrastructure.Mappers;

public class VenueHireApplicationMapper : IApplicationMapper
{
    private readonly IConcertOpportunityMapper opportunityMapper;

    public VenueHireApplicationMapper(IConcertOpportunityMapper opportunityMapper)
        => this.opportunityMapper = opportunityMapper;

    public IConcertApplication ToDto(ConcertApplicationEntity application)
    {
        var entity = (VenueHireApplicationEntity)application;
        return new VenueHireApplicationDto(entity.Id, entity.Artist.ToDto(), opportunityMapper.ToDto(entity.Opportunity), entity.Status);
    }

    public IConcertApplication ToArtistDto(ConcertApplicationEntity application)
    {
        var entity = (VenueHireApplicationEntity)application;
        return new VenueHireApplicationDto(entity.Id, entity.Artist.ToDto(), opportunityMapper.ToDto(entity.Opportunity), entity.Status);
    }
}
