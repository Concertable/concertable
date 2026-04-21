using Concertable.Application.Interfaces;
using Concertable.Identity.Contracts;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Shared.Exceptions;
using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;

namespace Concertable.Infrastructure.Services.Payment;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository purchaseRepository;
    private readonly ICurrentUser currentUser;
    private readonly ITransactionMapper transactionMapper;

    public TransactionService(
        ICurrentUser currentUser,
        ITransactionRepository purchaseRepository,
        ITransactionMapper transactionMapper)
    {
        this.currentUser = currentUser;
        this.purchaseRepository = purchaseRepository;
        this.transactionMapper = transactionMapper;
    }

    public async Task LogAsync(ITransaction dto)
    {
        var entity = transactionMapper.ToEntity(dto);
        await purchaseRepository.CreateAsync(entity);
    }

    public async Task CompleteAsync(string paymentIntentId)
    {
        var entity = await purchaseRepository.GetByPaymentIntentIdAsync(paymentIntentId);

        if (entity is null)
            return;

        entity.Complete();
        await purchaseRepository.SaveChangesAsync();
    }

    public async Task<IPagination<ITransaction>> GetAsync(IPageParams pageParams)
    {
        var userId = currentUser.GetId();
        var result = await purchaseRepository.GetAsync(pageParams, userId);
        var dtos = transactionMapper.ToDtos(result.Data);
        return new Pagination<ITransaction>(dtos, result.TotalCount, result.PageNumber, result.PageSize);
    }
}
