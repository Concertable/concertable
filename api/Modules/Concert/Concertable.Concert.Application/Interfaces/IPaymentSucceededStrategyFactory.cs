namespace Concertable.Concert.Application.Interfaces;

internal interface IPaymentSucceededStrategyFactory
{
    IPaymentSucceededStrategy Create(string type);
}
