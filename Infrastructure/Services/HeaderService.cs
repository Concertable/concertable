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

        public async virtual Task<PaginationResponse<TDto>> GetHeadersAsync(SearchParams searchParams)
        {
            var headers = await headerRepository.GetRawHeadersAsync(searchParams);

            // Apply location filtering only if all values are present
            if (searchParams.Latitude.HasValue && searchParams.Longitude.HasValue)
            {
                var radiusKm = searchParams.RadiusKm.GetValueOrDefault(10);
                headers.Data = locationService.FilterByRadius(
                    searchParams.Latitude.Value,
                    searchParams.Longitude.Value,
                    radiusKm,
                    headers.Data);
            }

            // Apply sorting only if sorting is "location_asc" or "location_desc"
            if (searchParams.Sort == "location_asc" || searchParams.Sort == "location_desc")
            {
                bool ascending = searchParams.Sort == "location_asc"; // true for asc, false for desc
                headers.Data = locationService.SortByDistance(
                    searchParams.Latitude.Value,
                    searchParams.Longitude.Value,
                    headers.Data,
                    ascending);
            }

            return new PaginationResponse<TDto>(
                headers.Data,
                headers.TotalCount,
                headers.PageNumber,
                headers.PageSize);
        }

        public async Task<IEnumerable<TDto>> GetHeadersAsync(int amount)
        {
            return await headerRepository.GetRawHeadersAsync(amount);
        }
    }
}
