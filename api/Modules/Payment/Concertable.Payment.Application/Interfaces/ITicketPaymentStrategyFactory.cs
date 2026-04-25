namespace Concertable.Payment.Application.Interfaces;

internal interface ITicketPaymentStrategyFactory
{
    ITicketPaymentStrategy Create(ContractType contractType);
}
