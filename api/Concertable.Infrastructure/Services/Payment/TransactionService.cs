using Application.Interfaces;
using Application.Interfaces.Payment;
using Application.Responses;
using Core.Entities;
using Core.Enums;
using Core.Exceptions;
using Core.Interfaces;
using Core.Parameters;

namespace Infrastructure.Services.Payment;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository purchaseRepository;
    private readonly ICurrentUser currentUser;
    private readonly ITransactionMapperFactory mapperFactory;

    public TransactionService(
        ICurrentUser currentUser,
        ITransactionRepository purchaseRepository,
        ITransactionMapperFactory mapperFactory)
    {
        this.currentUser = currentUser;
        this.purchaseRepository = purchaseRepository;
        this.mapperFactory = mapperFactory;
    }

    public async Task LogAsync(ITransaction dto)
    {
        var entity = mapperFactory.Create(dto.TransactionType).ToEntity(dto);
        await purchaseRepository.CreateAsync(entity);
    }

    public async Task CompleteAsync(string paymentIntentId)
    {
        var entity = await purchaseRepository.GetByPaymentIntentIdAsync(paymentIntentId)
            ?? throw new NotFoundException($"Transaction not found for PaymentIntent {paymentIntentId}");

        entity.Status = TransactionStatus.Complete;
        await purchaseRepository.SaveChangesAsync();
    }

    public async Task<Pagination<ITransaction>> GetAsync(IPageParams pageParams)
    {
        var userId = currentUser.GetId();
        var result = await purchaseRepository.GetAsync(pageParams, userId);
        var dtos = result.Data.Select(e => mapperFactory.Create(e.TransactionType).ToDto(e));
        return new Pagination<ITransaction>(dtos, result.TotalCount, result.PageNumber, result.PageSize);
    }
}
