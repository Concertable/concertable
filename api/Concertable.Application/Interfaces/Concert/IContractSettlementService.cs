using Core.Enums;

namespace Application.Interfaces.Concert;

public interface IContractSettlementService
{
    ContractType ContractType { get; }
    Task SettleAsync(int concertId);
}
