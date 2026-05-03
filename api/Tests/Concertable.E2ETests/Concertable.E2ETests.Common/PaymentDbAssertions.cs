using System.Data.Common;

namespace Concertable.E2ETests.Common;

public static class PaymentDbAssertions
{
    public static async Task<string?> GetLatestSettlementPaymentIntentIdByApplicationIdAsync(
        this DbConnection connection,
        int applicationId)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT TOP 1 t.PaymentIntentId
            FROM concert.Bookings b
            INNER JOIN payment.SettlementTransactions st ON st.BookingId = b.Id
            INNER JOIN payment.Transactions t ON t.Id = st.Id
            WHERE b.ApplicationId = @ApplicationId
              AND t.PaymentIntentId LIKE 'pi[_]%'
            ORDER BY t.CreatedAt DESC
            """;

        var p = cmd.CreateParameter();
        p.ParameterName = "@ApplicationId";
        p.Value = applicationId;
        cmd.Parameters.Add(p);

        return (string?)await cmd.ExecuteScalarAsync();
    }
}
