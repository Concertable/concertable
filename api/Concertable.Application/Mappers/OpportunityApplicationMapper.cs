using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public class OpportunityApplicationMapper : IOpportunityApplicationMapper
{
    private readonly IOpportunityMapper opportunityMapper;

    public OpportunityApplicationMapper(IOpportunityMapper opportunityMapper)
    {
        this.opportunityMapper = opportunityMapper;
    }

    public OpportunityApplicationDto ToDto(OpportunityApplicationEntity application) =>
        new(application.Id, application.Artist.ToSummaryDto(), opportunityMapper.ToDto(application.Opportunity), application.Opportunity.Contract.ContractType, application.Status);

    public IEnumerable<OpportunityApplicationDto> ToDtos(IEnumerable<OpportunityApplicationEntity> applications) =>
        applications.Select(ToDto);
}
