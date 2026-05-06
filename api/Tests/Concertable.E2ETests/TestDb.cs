using System.Data.Common;
using Dapper;

namespace Concertable.E2ETests;

public class TestDb(DbConnection connection)
{
    public OpportunityDb Opportunity { get; } = new(connection);
    public PaymentDb Payment { get; } = new(connection);
}

public class OpportunityDb(DbConnection connection)
{
    public Task<int> GetNewestAsync(int venueId) =>
        connection.QuerySingleAsync<int>(
            "SELECT MAX(Id) FROM concert.Opportunities WHERE VenueId = @venueId",
            new { venueId });
}

public class PaymentDb(DbConnection connection)
{
    // payment DB queries go here
}
