using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Core.Exceptions;
using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;

namespace Concertable.Infrastructure.Services.Payment;

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
        var entity = await purchaseRepository.GetByPaymentIntentIdAsync(paymentIntentId);

        if (entity is null)
            return;

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
