using System.Data.Common;
using Dapper;

namespace Concertable.E2ETests;

public class TestDb(DbConnection connection, DbConnection paymentConnection)
{
    public OpportunityDb Opportunity { get; } = new(connection);
    public BookingDb Booking { get; } = new(connection);
    public PaymentDb Payment { get; } = new(paymentConnection);
}

public class OpportunityDb(DbConnection connection)
{
    public Task<int> GetNewestAsync(int venueId) =>
        connection.QuerySingleAsync<int>(
            "SELECT MAX(Id) FROM concert.Opportunities WHERE VenueId = @venueId",
            new { venueId });
}

public class BookingDb(DbConnection connection)
{
    public Task<int> GetStatusByApplicationIdAsync(int applicationId) =>
        connection.QuerySingleAsync<int>(
            "SELECT Status FROM concert.Bookings WHERE ApplicationId = @applicationId",
            new { applicationId });
}

public class PaymentDb(DbConnection connection)
{
    public Task<PayoutAccountRow?> GetPayoutAccountByUserIdAsync(Guid userId) =>
        connection.QuerySingleOrDefaultAsync<PayoutAccountRow?>(
            "SELECT StripeAccountId, StripeCustomerId FROM payment.PayoutAccounts WHERE UserId = @userId",
            new { userId });
}

public record PayoutAccountRow(string? StripeAccountId, string? StripeCustomerId);
