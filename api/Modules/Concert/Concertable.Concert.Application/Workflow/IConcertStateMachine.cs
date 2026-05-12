using Concertable.Concert.Application.Workflow.Steps;

namespace Concertable.Concert.Application.Workflow;

internal interface IConcertStateMachine
{
    Task GuardAsync<TStep>(int lifecycleId) where TStep : IConcertStep;
    Task AdvanceAsync<TStep>(int lifecycleId) where TStep : IConcertStep;
}
