using Application.DTOs;
using Application.Interfaces;
using Application.Responses;
using Core.Parameters;
using Infrastructure.Repositories;
using Stripe.Terminal;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using Infrastructure.Helpers;
using Core.Interfaces;

namespace Infrastructure.Services
{
    public class HeaderService<TEntity, TDto, TRepository> : IHeaderService<TDto>
        where TEntity : BaseEntity, ILocation
        where TDto : HeaderDto
        where TRepository : IHeaderRepository<TEntity, TDto>
    {
        private readonly TRepository headerRepository;
        private readonly ILocationService locationService;

        protected HeaderService(
           TRepository headerRepository,
           ILocationService locationService)
        {
            this.headerRepository = headerRepository;
            this.locationService = locationService;
        }

        public async virtual Task<PaginationResponse<TDto>> GetHeadersAsync(SearchParams searchParams)
        {
            var headers = await headerRepository.GetHeadersAsync(searchParams);

            return new PaginationResponse<TDto>(
                headers.Data,
                headers.TotalCount,
                headers.PageNumber,
                headers.PageSize);
        }

        public async Task<IEnumerable<TDto>> GetHeadersAsync(int amount)
        {
            return await headerRepository.GetHeadersAsync(amount);
        }
    }
}
