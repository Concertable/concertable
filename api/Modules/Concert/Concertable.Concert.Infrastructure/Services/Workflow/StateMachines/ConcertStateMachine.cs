using Concertable.Concert.Application.Workflow;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Workflow.StateMachines;

internal abstract class ConcertStateMachine : IConcertStateMachine
{
    protected abstract ConcertStage[] Sequence { get; }

    public bool CanTransitionTo(ConcertStage target, ConcertStage current)
    {
        var ci = Array.IndexOf(Sequence, current);
        var ti = Array.IndexOf(Sequence, target);
        return ci >= 0 && ti == ci + 1;
    }

    public ConcertStage NextStage(ConcertStage current)
    {
        var ci = Array.IndexOf(Sequence, current);
        if (ci < 0 || ci + 1 >= Sequence.Length)
            throw new BadRequestException($"No next stage from {current}");
        return Sequence[ci + 1];
    }
}
