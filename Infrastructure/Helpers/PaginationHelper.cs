using Core.Interfaces;
﻿using Core.Parameters;
using Application.Responses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Infrastructure.Helpers
{
    public static class PaginationHelper
    {
        public static async Task<Pagination<T>> CreatePaginatedResponseAsync<T>(
        IQueryable<T> query, IPageParams pageParams)
        {
            int totalCount = await query.CountAsync();
            var data = await query
                .Skip((pageParams.PageNumber - 1) * pageParams.PageSize)
                .Take(pageParams.PageSize)
                .ToListAsync();

            return new Pagination<T>(data, totalCount, pageParams.PageNumber, pageParams.PageSize);
        }

        public static async Task<Pagination<T>> ToPaginationAsync<T>(
            this IQueryable<T> query, IPageParams pageParams)
            => await CreatePaginatedResponseAsync(query, pageParams);
    }
}
