using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Enums;

namespace Concertable.Application.Interfaces;

public interface IContractStrategyFactory<T> where T : IContractStrategy
{
    T Create(ContractType contractType);
}
