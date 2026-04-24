namespace Concertable.Contract.Application.Interfaces;

internal interface IContractMapper
{
    IContract ToContract(ContractEntity entity);
}
