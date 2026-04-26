using Concertable.Application.Interfaces;
using Concertable.Infrastructure.Data;
using Concertable.Shared.Enums;
using Concertable.Shared.Exceptions;
using Concertable.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Concertable.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext context;

    public UnitOfWork(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }

    public async Task TrySaveChangesAsync()
    {
        try
        {
            await SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsDuplicateKey())
        {
            throw new BadRequestException("A record with this Key already exists", ErrorType.DuplicateKey);
        }
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await context.Database.BeginTransactionAsync();
    }

    public void Dispose()
    {
        context.Dispose();
    }
}
