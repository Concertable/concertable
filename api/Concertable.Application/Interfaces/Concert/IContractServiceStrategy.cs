using Concertable.Core.Entities.Contracts;

namespace Concertable.Application.Interfaces.Concert;

public interface IContractServiceStrategy : IContractStrategy
{
    void ApplyChanges(ContractEntity existing, IContract dto);
}
