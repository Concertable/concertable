using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Results;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Helpers;
using Concertable.Core.Entities;
using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories;

public class TransactionRepository : Repository<TransactionEntity>, ITransactionRepository
{
    public TransactionRepository(ApplicationDbContext context) : base(context) { }

    public Task<Pagination<TransactionEntity>> GetAsync(IPageParams pageParams, Guid userId)
    {
        var query = context.Transactions
            .Where(t => t.FromUserId == userId || t.ToUserId == userId)
            .OrderByDescending(t => t.CreatedAt);

        return query.ToPaginationAsync(pageParams);
    }

    public Task<TransactionEntity?> GetByPaymentIntentIdAsync(string paymentIntentId) =>
        context.Transactions.FirstOrDefaultAsync(t => t.PaymentIntentId == paymentIntentId);

    public async Task CreateAsync(TransactionEntity entity)
    {
        await context.Transactions.AddAsync(entity);
        await context.SaveChangesAsync();
    }
}
