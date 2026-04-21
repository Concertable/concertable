using Concertable.Application.Interfaces;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces.Payment;

public interface ITransactionRepository : IRepository<TransactionEntity>
{
    Task<IPagination<TransactionEntity>> GetAsync(IPageParams pageParams, Guid userId);
    Task<TransactionEntity?> GetByPaymentIntentIdAsync(string paymentIntentId);
    Task CreateAsync(TransactionEntity entity);
}
