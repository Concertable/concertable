using Concertable.Contract.Application.Interfaces;
using Concertable.Contract.Infrastructure.Data;

namespace Concertable.Contract.Infrastructure.Repositories;

internal class ContractRepository(ContractDbContext context)
    : Repository<ContractEntity, ContractDbContext>(context), IContractRepository
{
}
