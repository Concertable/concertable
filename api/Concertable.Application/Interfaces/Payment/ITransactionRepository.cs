using Application.Interfaces;
using Application.Responses;
using Core.Entities;
using Core.Interfaces;
using Core.Parameters;

namespace Application.Interfaces.Payment;

public interface ITransactionRepository : IRepository<TransactionEntity>
{
    Task<Pagination<TransactionEntity>> GetAsync(IPageParams pageParams, int userId);
    Task<TransactionEntity?> GetByPaymentIntentIdAsync(string paymentIntentId);
    Task CreateAsync(TransactionEntity entity);
}
