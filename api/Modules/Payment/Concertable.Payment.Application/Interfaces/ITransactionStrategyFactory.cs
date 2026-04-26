namespace Concertable.Payment.Application.Interfaces;

internal interface ITransactionStrategyFactory
{
    ITransactionStrategy Create(string type);
}
