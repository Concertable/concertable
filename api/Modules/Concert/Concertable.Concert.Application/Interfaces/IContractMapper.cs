namespace Concertable.Concert.Application.Interfaces;

internal interface IContractMapper : IContractStrategy
{
    ContractEntity ToEntity(IContract dto);
    IContract ToDto(ContractEntity entity);
}
