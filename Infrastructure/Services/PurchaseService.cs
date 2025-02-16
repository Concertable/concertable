using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository purchaseRepository;
        private readonly IMapper mapper;

        public PurchaseService(IMapper mapper, IPurchaseRepository purchaseRepository)
        {
            this.purchaseRepository = purchaseRepository;
            this.mapper = mapper;
        }

        public async Task LogAsync(PurchaseDto purchaseDto)
        {
            var purchase = mapper.Map<Purchase>(purchaseDto);

            await purchaseRepository.AddAsync(purchase);
            await purchaseRepository.SaveChangesAsync();
        }
    }
}
