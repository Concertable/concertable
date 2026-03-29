using Concertable.Application.Requests;

namespace Concertable.Web.IntegrationTests.Controllers.Concert;

public static class ConcertRequestBuilders
{
    public static UpdateConcertRequest BuildPostRequest(
        string name = "Test Concert",
        string about = "Test Concert About",
        decimal price = 10.00m,
        int totalTickets = 100) => new()
    {
        Name = name,
        About = about,
        Price = price,
        TotalTickets = totalTickets
    };
}
