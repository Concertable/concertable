using Concertable.Core.Entities;
using Concertable.Core.Enums;
using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.Seeding.Factories;

public static class OpportunityApplicationFactory
{
    public static OpportunityApplicationEntity Accepted(int artistId, int opportunityId, ConcertEntity concert)
    {
        var app = New<OpportunityApplicationEntity>()
            .With(nameof(OpportunityApplicationEntity.ArtistId), artistId)
            .With(nameof(OpportunityApplicationEntity.OpportunityId), opportunityId)
            .With(nameof(OpportunityApplicationEntity.Status), ApplicationStatus.Accepted);
        app.Concert = concert;
        return app;
    }

    public static OpportunityApplicationEntity Complete(int artistId, int opportunityId, ConcertEntity concert)
    {
        var app = New<OpportunityApplicationEntity>()
            .With(nameof(OpportunityApplicationEntity.ArtistId), artistId)
            .With(nameof(OpportunityApplicationEntity.OpportunityId), opportunityId)
            .With(nameof(OpportunityApplicationEntity.Status), ApplicationStatus.Complete);
        app.Concert = concert;
        return app;
    }

    public static OpportunityApplicationEntity AwaitingPayment(int artistId, int opportunityId, ConcertEntity concert)
    {
        var app = New<OpportunityApplicationEntity>()
            .With(nameof(OpportunityApplicationEntity.ArtistId), artistId)
            .With(nameof(OpportunityApplicationEntity.OpportunityId), opportunityId)
            .With(nameof(OpportunityApplicationEntity.Status), ApplicationStatus.AwaitingPayment);
        app.Concert = concert;
        return app;
    }


    public static OpportunityApplicationEntity Create(int artistId, int opportunityId)
        => OpportunityApplicationEntity.Create(artistId, opportunityId);

    public static OpportunityApplicationEntity Accepted(
        int artistId,
        int opportunityId,
        string concertName,
        string concertAbout,
        IEnumerable<int> genreIds)
    {
        var app = OpportunityApplicationEntity.Create(artistId, opportunityId);
        var concert = ConcertEntity.CreateDraft(0, concertName, concertAbout, genreIds);
        app.Accept(concert);
        return app;
    }

    public static OpportunityApplicationEntity Accepted(
        int artistId,
        int opportunityId,
        string concertName,
        string concertAbout,
        IEnumerable<int> genreIds,
        decimal price,
        int totalTickets,
        DateTime now)
    {
        var app = OpportunityApplicationEntity.Create(artistId, opportunityId);
        var concert = ConcertEntity.CreateDraft(0, concertName, concertAbout, genreIds);
        concert.Post(concertName, concertAbout, price, totalTickets, now);
        app.Accept(concert);
        return app;
    }

    public static OpportunityApplicationEntity AwaitingPayment(
        int artistId,
        int opportunityId,
        string concertName,
        string concertAbout,
        IEnumerable<int> genreIds)
    {
        var app = OpportunityApplicationEntity.Create(artistId, opportunityId);
        var concert = ConcertEntity.CreateDraft(0, concertName, concertAbout, genreIds);
        app.Accept(concert);
        app.AwaitPayment();
        return app;
    }
}
