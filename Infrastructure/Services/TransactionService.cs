using Application.DTOs;
using Application.Interfaces;
using Application.Responses;
using AutoMapper;
using Core.Entities;
using Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository purchaseRepository;
        private readonly ICurrentUserService currentUserService;
        private readonly IMapper mapper;

        public TransactionService(
            IMapper mapper,
            ICurrentUserService currentUserService,
            ITransactionRepository purchaseRepository)
        {
            this.purchaseRepository = purchaseRepository;
            this.currentUserService = currentUserService;
            this.mapper = mapper;
        }

        public async Task LogAsync(TransactionDto purchaseDto)
        {
            var purchase = mapper.Map<Transaction>(purchaseDto);

            await purchaseRepository.AddAsync(purchase);
            await purchaseRepository.SaveChangesAsync();
        }

        public async Task<PaginationResponse<TransactionDto>> GetAsync(PaginationParams pageParams)
        {
            var userId = await currentUserService.GetIdAsync();

            var result = await purchaseRepository.GetAsync(pageParams, userId);

            var resultDto = mapper.Map<IEnumerable<TransactionDto>>(result.Data);

            return new PaginationResponse<TransactionDto>(
                resultDto,
                result.TotalCount,
                result.PageNumber,
                result.PageSize);
        }
    }
}
