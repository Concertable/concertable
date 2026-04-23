namespace Concertable.Concert.Application.Interfaces;

internal interface IContractStrategyFactory<T> where T : IContractStrategy
{
    T Create(ContractType contractType);
}
