using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public class ConcertApplicationMapper : IConcertApplicationMapper
{
    private readonly IConcertOpportunityMapper opportunityMapper;

    public ConcertApplicationMapper(IConcertOpportunityMapper opportunityMapper)
    {
        this.opportunityMapper = opportunityMapper;
    }

    public ConcertApplicationDto ToDto(ConcertApplicationEntity application) =>
        new(application.Id, application.Artist.ToSummaryDto(), opportunityMapper.ToDto(application.Opportunity), application.Opportunity.Contract.ContractType, application.Status);

    public IEnumerable<ConcertApplicationDto> ToDtos(IEnumerable<ConcertApplicationEntity> applications) =>
        applications.Select(ToDto);
}
