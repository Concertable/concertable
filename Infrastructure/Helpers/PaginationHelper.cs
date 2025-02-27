using Core.Parameters;
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
        public static async Task<PaginationResponse<T>> CreatePaginatedResponseAsync<T>(
        IQueryable<T> query, PaginationParams pageParams)
        {
            int totalCount = await query.CountAsync();
            var data = await query.Skip((pageParams.PageNumber - 1) * pageParams.PageSize).Take(pageParams.PageSize).ToListAsync();

            return new PaginationResponse<T>(data, totalCount, pageParams.PageNumber, pageParams.PageSize);
        }

        public static PaginationParams CreateDefaultSummaryParams() => new PaginationParams {PageNumber = 1, PageSize = 5};

        public static PaginationParams CreateDefaultSearchParams() => new PaginationParams { PageNumber = 1, PageSize = 10 };
    }
}
