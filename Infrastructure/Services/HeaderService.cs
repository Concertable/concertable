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

namespace Infrastructure.Services
{
    public class HeaderService<TEntity, TDto, TRepository> : IHeaderService<TDto>
        where TEntity : BaseEntity
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

        public async virtual Task<PaginationResponse<TDto>> GetHeadersAsync(SearchParams? searchParams)
        {
            var headers = await headerRepository.GetRawHeadersAsync(searchParams);

            var locationHeaders = locationService.FilterAndSortByNearest(searchParams, headers.Data);

            return new PaginationResponse<TDto>(
                locationHeaders,
                headers.TotalCount,
                headers.PageNumber,
                headers.PageSize);
        }
    }
}
