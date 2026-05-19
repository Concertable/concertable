using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Shared.Infrastructure.Extensions;

public static class DbUpdateExceptionExtensions
{
    public static bool IsDuplicateKey(this DbUpdateException ex)
    {
        return ex.InnerException is SqlException sqlEx &&
            (sqlEx.Number == 2601 || sqlEx.Number == 2627);
    }
}
