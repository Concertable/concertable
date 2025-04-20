using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public static class SqlExceptionHelper
    {
        public static bool IsDuplicateKey(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx &&
           (sqlEx.Number == 2601 || sqlEx.Number == 2627);
        }
    }
}
