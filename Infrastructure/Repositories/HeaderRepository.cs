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

namespace Infrastructure.Repositories
{
    public abstract class HeaderRepository<TEntity, TDto> : Repository<TEntity>, IHeaderRepository<TEntity, TDto>
        where TDto : HeaderDto
        where TEntity : BaseEntity
    {
        private readonly ApplicationDbContext context;
        private readonly DbSet<TEntity> dbSet;

        public HeaderRepository(
            ApplicationDbContext context): base(context)
        {
            this.dbSet = context.Set<TEntity>();
        }

        protected abstract Expression<Func<TEntity, TDto>> Selector { get; }
        protected virtual List<Expression<Func<TEntity, bool>>> Filters(SearchParams searchParams) => new();

        protected virtual IQueryable<TDto> GetRawHeadersQuery(SearchParams? searchParams)
        {
            var query = dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchParams?.SearchTerm))
                query = query.Where(e => EF.Property<string>(e, "Name").Contains(searchParams.SearchTerm));

            foreach (var filter in Filters(searchParams))
                query = query.Where(filter);

            if (!string.IsNullOrWhiteSpace(searchParams?.Sort))
                query = ApplyOrdering(query, searchParams.Sort);

            return query.Select(Selector);
        }

        public async Task<PaginationResponse<TDto>> GetRawHeadersAsync(SearchParams? searchParams)
        {
            var result = GetRawHeadersQuery(searchParams);

            return await PaginationHelper.CreatePaginatedResponseAsync(result, searchParams);
        }

        protected virtual IQueryable<TDto> GetRawHeadersQuery(int amount)
        {
            return dbSet.Take(amount).Select(Selector);
        }

        public async Task<IEnumerable<TDto>> GetRawHeadersAsync(int amount)
        {
            return await GetRawHeadersQuery(amount).ToListAsync();
        }

        protected virtual IQueryable<TEntity> ApplyOrdering(IQueryable<TEntity> query, string? sort)
        {
            return sort?.ToLower() switch
            {
                "name_asc" => query.OrderBy(e => EF.Property<string>(e, "Name")),
                "name_desc" => query.OrderByDescending(e => EF.Property<string>(e, "Name")),
                _ => query
            };
        }
    }
}
