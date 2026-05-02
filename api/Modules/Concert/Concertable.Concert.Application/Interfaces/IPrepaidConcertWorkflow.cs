namespace Concertable.Concert.Application.Interfaces;

internal interface IPrepaidConcertWorkflow
    : IConcertWorkflow, IApplyWithPaymentMethod, IAcceptByConfirmation
{
}
