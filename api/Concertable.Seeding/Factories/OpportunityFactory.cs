using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.Seeding.Factories;

public static class OpportunityFactory
{
    public static OpportunityEntity Create(int venueId, DateRange period, int contractId, IEnumerable<int>? genreIds = null)
    {
        var opp = New<OpportunityEntity>()
            .With(nameof(OpportunityEntity.VenueId), venueId)
            .With(nameof(OpportunityEntity.Period), period)
            .With(nameof(OpportunityEntity.ContractId), contractId);
        if (genreIds is not null)
            opp.SyncGenres(genreIds);
        return opp;
    }
}
