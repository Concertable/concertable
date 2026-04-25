using Concertable.Application.Interfaces;

namespace Concertable.Payment.Application.Interfaces;

internal interface ITransactionRepository : IIdRepository<TransactionEntity>
{
    Task<IPagination<TransactionEntity>> GetAsync(IPageParams pageParams, Guid userId);
    Task<TransactionEntity?> GetByPaymentIntentIdAsync(string paymentIntentId);
    Task CreateAsync(TransactionEntity entity);
}
