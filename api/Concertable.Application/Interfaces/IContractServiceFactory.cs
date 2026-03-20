using Application.Interfaces.Concert;
using Core.Enums;

namespace Application.Interfaces;

public interface IContractServiceFactory<T> where T : class, IContractWorkflow
{
    T Create(ContractType contractType);
}
