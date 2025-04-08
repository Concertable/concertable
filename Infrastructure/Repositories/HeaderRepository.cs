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

        public async Task<PaginationResponse<TDto>> GetRawHeadersAsync(SearchParams? searchParams)
        {
            var query = dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchParams?.SearchTerm))
                query = query.Where(e => EF.Property<string>(e, "Name").Contains(searchParams.SearchTerm));

            foreach (var filter in Filters(searchParams))
                query = query.Where(filter);

            if (!string.IsNullOrWhiteSpace(searchParams?.Sort))
                query = ApplyOrdering(query, searchParams.Sort);

            var result = query.Select(Selector);

            return await PaginationHelper.CreatePaginatedResponseAsync(result, searchParams);
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
