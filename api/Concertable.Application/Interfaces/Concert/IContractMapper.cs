using Concertable.Core.Entities.Contracts;
using Core.Enums;

namespace Application.Interfaces.Concert;

public interface IContractMapper : IContractWorkflow
{
    ContractType ContractType { get; }
    ContractEntity ToEntity(IContract dto);
    IContract ToDto(ContractEntity entity);
}
