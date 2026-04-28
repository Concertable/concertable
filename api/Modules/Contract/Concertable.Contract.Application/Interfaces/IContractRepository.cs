using Concertable.Application.Interfaces;

namespace Concertable.Contract.Application.Interfaces;

internal interface IContractRepository : IIdRepository<ContractEntity>
{
    Task<IEnumerable<ContractEntity>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
}
