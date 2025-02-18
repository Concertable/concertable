using Core.Responses;
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
        IQueryable<T> query, int pageNumber, int pageSize)
        {
            int totalCount = await query.CountAsync();
            var data = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginationResponse<T>(data, totalCount, pageNumber, pageSize);
        }

    }
}
