using Concertable.Application.Interfaces;
using Concertable.Application.Results;
using Concertable.Core.Entities;
using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces.Payment;

public interface ITransactionRepository : IRepository<TransactionEntity>
{
    Task<Pagination<TransactionEntity>> GetAsync(IPageParams pageParams, Guid userId);
    Task<TransactionEntity?> GetByPaymentIntentIdAsync(string paymentIntentId);
    Task CreateAsync(TransactionEntity entity);
}
