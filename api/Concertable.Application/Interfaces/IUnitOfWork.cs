using Microsoft.EntityFrameworkCore.Storage;

namespace Concertable.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task SaveChangesAsync();
    Task TrySaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}
