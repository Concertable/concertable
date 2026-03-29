using Concertable.Core.Enums;

namespace Concertable.Application.Interfaces.Concert;

public interface IContractMapperFactory
{
    IContractMapper Create(ContractType type);
}
