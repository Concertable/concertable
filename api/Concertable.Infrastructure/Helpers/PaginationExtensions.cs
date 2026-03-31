using Concertable.Core.Interfaces;
using Concertable.Application.Responses;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Helpers;

public static class PaginationExtensions
{
    public static async Task<Pagination<T>> ToPaginationAsync<T>(
        this IQueryable<T> query, IPageParams pageParams)
    {
        int totalCount = await query.CountAsync();
        var data = await query
            .Skip((pageParams.PageNumber - 1) * pageParams.PageSize)
            .Take(pageParams.PageSize)
            .ToListAsync();

        return new Pagination<T>(data, totalCount, pageParams.PageNumber, pageParams.PageSize);
    }
}
