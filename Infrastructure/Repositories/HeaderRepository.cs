using Application.DTOs;
using Application.Interfaces;
using Core.Parameters;
using Core.Responses;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class HeaderRepository<TEntity, TDto> : IHeaderRepository<TDto>
        where TDto : HeaderDto
        where TEntity : class
    {
        private readonly ApplicationDbContext context;
        private readonly DbSet<TEntity> dbSet;
        private readonly Expression<Func<TEntity, TDto>> selector;
        private readonly List<Expression<Func<TEntity, bool>>> filters; // ✅ Use Entity-level filters

        public HeaderRepository(
            ApplicationDbContext context,
            Expression<Func<TEntity, TDto>> selector,
            List<Expression<Func<TEntity, bool>>> filters) // ✅ Accept entity filters
        {
            this.context = context;
            this.dbSet = context.Set<TEntity>();
            this.selector = selector;
            this.filters = filters;
        }

        public HeaderRepository(
            ApplicationDbContext context,
            Expression<Func<TEntity, TDto>> selector)
        {
            this.context = context;
            this.dbSet = context.Set<TEntity>();
            this.selector = selector;
            this.filters = new List<Expression<Func<TEntity, bool>>>();
        }

        public async Task<PaginationResponse<TDto>> GetRawHeadersAsync(SearchParams searchParams)
        {
            var query = dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchParams?.SearchTerm))
            {
                query = query.Where(e => EF.Property<string>(e, "Name").Contains(searchParams.SearchTerm));
            }

            foreach (var filter in filters)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(searchParams?.Sort))
            {
                query = searchParams.Sort.ToLower() switch
                {
                    "name_asc" => query.OrderBy(e => EF.Property<string>(e, "Name")),
                    "name_desc" => query.OrderByDescending(e => EF.Property<string>(e, "Name")),
                    _ => query
                };
            }

            var result = query.Select(selector);

            return await PaginationHelper.CreatePaginatedResponseAsync(result, searchParams);
        }
    }
}
