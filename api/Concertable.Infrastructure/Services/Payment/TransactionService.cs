using Core.Interfaces;
using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Payment;
using Application.Mappers;
using Application.Responses;
using Core.Entities;
using Core.Parameters;

namespace Infrastructure.Services.Payment;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository purchaseRepository;
    private readonly ICurrentUser currentUser;

    public TransactionService(
        ICurrentUser currentUser,
        ITransactionRepository purchaseRepository)
    {
        this.purchaseRepository = purchaseRepository;
        this.currentUser = currentUser;
    }

    public async Task LogAsync(TransactionDto purchaseDto)
    {
        var purchase = purchaseDto.ToEntity();

        await purchaseRepository.AddAsync(purchase);
        await purchaseRepository.SaveChangesAsync();
    }

    public async Task<Pagination<TransactionDto>> GetAsync(IPageParams pageParams)
    {
        var userId = currentUser.GetId();
        var result = await purchaseRepository.GetAsync(pageParams, userId);

        return new Pagination<TransactionDto>(
            result.Data.ToDtos(),
            result.TotalCount,
            result.PageNumber,
            result.PageSize);
    }
}
