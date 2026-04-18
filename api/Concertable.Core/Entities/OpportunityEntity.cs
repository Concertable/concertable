using Concertable.Core.Entities.Contracts;
using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Extensions;
using Concertable.Core.Interfaces;
using Concertable.Core.ValueObjects;

namespace Concertable.Core.Entities;

public class OpportunityEntity : IIdEntity, IHasGenreJoins<OpportunityGenreEntity>
{
    private OpportunityEntity() { }

    public int Id { get; private set; }
    public int VenueId { get; set; }
    public DateRange Period { get; private set; } = null!;
    public VenueEntity Venue { get; set; } = null!;
    public ContractEntity Contract { get; private set; } = null!;
    public HashSet<OpportunityApplicationEntity> Applications { get; private set; } = [];
    public HashSet<OpportunityGenreEntity> OpportunityGenres { get; private set; } = [];

    HashSet<OpportunityGenreEntity> IHasGenreJoins<OpportunityGenreEntity>.GenreJoins => OpportunityGenres;

    public static OpportunityEntity Create(int venueId, DateRange period, ContractEntity contract, IEnumerable<int>? genreIds = null)
    {
        var opportunity = new OpportunityEntity
        {
            VenueId = venueId,
            Period = period,
            Contract = contract
        };

        if (genreIds is not null)
            opportunity.SyncGenres(genreIds);

        return opportunity;
    }

    public void Update(DateRange period, ContractEntity contract, IEnumerable<int> genreIds)
    {
        Period = period;
        Contract = contract;
        SyncGenres(genreIds);
    }

    public void SyncGenres(IEnumerable<int> genreIds) =>
        this.SyncGenres<OpportunityGenreEntity>(genreIds);
}
