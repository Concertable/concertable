using Concertable.Core.Entities.Contracts;
using Concertable.Core.Enums;

namespace Concertable.Application.Interfaces.Concert;

public interface IContractMapper : IContractStrategy
{
    ContractType ContractType { get; }
    ContractEntity ToEntity(IContract dto);
    IContract ToDto(ContractEntity entity);
}
