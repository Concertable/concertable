using Concertable.Core.Entities;
using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.Seeding.Factories;

public static class OpportunityFactory
{
    public static OpportunityEntity Create(int venueId, DateRange period, ContractEntity contract, IEnumerable<int>? genreIds = null)
    {
        var opp = New<OpportunityEntity>()
            .With(nameof(OpportunityEntity.VenueId), venueId)
            .With(nameof(OpportunityEntity.Period), period)
            .With(nameof(OpportunityEntity.Contract), contract);
        if (genreIds is not null)
            opp.SyncGenres(genreIds);
        return opp;
    }
}
