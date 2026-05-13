using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Application.Workflow.Executors;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Executors;

internal class SettleExecutor : ISettleExecutor
{
    private readonly IStepExecutor<BookingEntity> stepExecutor;
    private readonly IConcertWorkflowFactory workflows;

    public SettleExecutor(IStepExecutor<BookingEntity> stepExecutor, IConcertWorkflowFactory workflows)
    {
        this.stepExecutor = stepExecutor;
        this.workflows = workflows;
    }

    public Task ExecuteAsync(int bookingId)
        => stepExecutor.ExecuteAsync(bookingId, ConcertStage.Settled, Dispatch);

    private Task Dispatch(BookingEntity booking)
    {
        var workflow = workflows.Create(booking.ContractType);
        return workflow.Settle.ExecuteAsync(booking.Id);
    }
}
