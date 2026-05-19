using Microsoft.EntityFrameworkCore.Storage;

namespace Concertable.Application.Interfaces;

public interface IUnitOfWork<TContext>
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
