using Application.Interfaces.Concert;
using Core.Enums;

namespace Application.Interfaces;

public interface IContractStrategyFactory<T> where T : IContractStrategy
{
    T Create(ContractType contractType);
}
