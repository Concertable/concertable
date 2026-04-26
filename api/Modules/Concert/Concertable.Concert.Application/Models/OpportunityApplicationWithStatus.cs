namespace Concertable.Concert.Application.Models;

internal class OpportunityApplicationWithStatus
{
    public required OpportunityApplicationEntity Application { get; set; }
    public bool HasConcert { get; set; }
}
