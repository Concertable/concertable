namespace Concertable.Concert.Application.Interfaces;

internal interface IPaymentSucceededProcessorFactory
{
    IPaymentSucceededProcessor Create(string type);
}
