using Core.Enums;

namespace Application.Interfaces.Concert;

public interface IContractMapperFactory
{
    IContractMapper Create(ContractType type);
}
