using Application.DTOs;
using Application.Interfaces;
using Core.Parameters;
using Application.Responses;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Core.Entities;
using System.Collections;
using NetTopologySuite.Algorithm;
using Core.Interfaces;
using NetTopologySuite.Geometries;
using Stripe.Sigma;

namespace Infrastructure.Repositories
{
    public abstract class HeaderRepository<TEntity, TDto> : Repository<TEntity>, IHeaderRepository<TEntity, TDto>
    where TDto : HeaderDto
    where TEntity : BaseEntity, ILocation
    {
        private readonly ApplicationDbContext context;
        private readonly DbSet<TEntity> dbSet;

        public HeaderRepository(ApplicationDbContext context) : base(context)
        {
            this.dbSet = context.Set<TEntity>();
        }

        protected abstract Expression<Func<TEntity, TDto>> Selector { get; }

        protected virtual List<Expression<Func<TEntity, bool>>> Filters(SearchParams searchParams) => new();

        // Add this method to handle radius-based filtering
        protected abstract IQueryable<TEntity> FilterByRadius(IQueryable<TEntity> query, double latitude, double longitude, double radiusKm);

        protected virtual IQueryable<TDto> GetHeadersQuery(SearchParams? searchParams)
        {
            var query = dbSet.AsQueryable();

            // Search term filtering
            if (!string.IsNullOrWhiteSpace(searchParams?.SearchTerm))
                query = query.Where(e => EF.Property<string>(e, "Name").Contains(searchParams.SearchTerm));

            // Apply additional filters from SearchParams
            foreach (var filter in Filters(searchParams))
                query = query.Where(filter);

            // Apply ordering if present
            if (!string.IsNullOrWhiteSpace(searchParams?.Sort))
                query = ApplyOrdering(query, searchParams.Sort);

            // If location parameters are provided, filter by radius
            if (GeoHelper.HasValidCoordinates(searchParams))
                query = FilterByRadius(query, searchParams.Latitude!.Value, searchParams.Longitude!.Value, searchParams.RadiusKm ?? 10);

            return query.Select(Selector);
        }

        protected virtual IQueryable<TDto> GetHeadersQuery(int amount)
        {
            return dbSet.Select(Selector).Take(amount);
        }

        // Handle pagination
        public async Task<PaginationResponse<TDto>> GetHeadersAsync(SearchParams? searchParams)
        {
            var result = GetHeadersQuery(searchParams);
            return await PaginationHelper.CreatePaginatedResponseAsync(result, searchParams);
        }

        public async Task<IEnumerable<TDto>> GetHeadersAsync(int amount)
        {
            var query = GetHeadersQuery(amount);
            return await query.ToListAsync();
        }

        // Default sorting method
        protected virtual IQueryable<TEntity> ApplyOrdering(IQueryable<TEntity> query, string? sort)
        {
            return sort?.ToLower() switch
            {
                "name_asc" => query.OrderBy(h => EF.Property<string>(h, "Name")),
                "name_desc" => query.OrderByDescending(h => EF.Property<string>(h, "Name")),
                _ => query.OrderBy(h => h.Id)
            };
        }


    }

}
