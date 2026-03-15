using Microsoft.EntityFrameworkCore.Storage;

namespace Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task SaveChangesAsync();
    Task TrySaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}
