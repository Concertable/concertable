using Concertable.Concert.Application.Workflow.Steps;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Steps;

internal class VenueHirePaidApplyStep : IPaidApplyStep
{
    public Task ExecuteAsync(ApplicationEntity app) => Task.CompletedTask;
}
