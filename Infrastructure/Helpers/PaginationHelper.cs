using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public static class PaginationHelper
    {
        public static IQueryable<T> ApplyPagination<T>(IQueryable<T> query, int page, int pageSize) where T : class
        {
            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }
    }
}
