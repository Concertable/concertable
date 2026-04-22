
namespace Concertable.Application.Interfaces.Concert;

public interface IContractMapper : IContractStrategy
{
    ContractEntity ToEntity(IContract dto);
    IContract ToDto(ContractEntity entity);
}
