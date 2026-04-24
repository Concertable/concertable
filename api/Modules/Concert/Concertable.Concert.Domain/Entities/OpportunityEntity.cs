namespace Concertable.Concert.Domain;

public class OpportunityEntity : IIdEntity, IHasGenreJoins<OpportunityGenreEntity>
{
    private OpportunityEntity() { }

    public int Id { get; private set; }
    public int VenueId { get; set; }
    public DateRange Period { get; private set; } = null!;
    public VenueReadModel Venue { get; set; } = null!;
    public int ContractId { get; private set; }
    public HashSet<OpportunityApplicationEntity> Applications { get; private set; } = [];
    public HashSet<OpportunityGenreEntity> OpportunityGenres { get; private set; } = [];

    HashSet<OpportunityGenreEntity> IHasGenreJoins<OpportunityGenreEntity>.GenreJoins => OpportunityGenres;

    public static OpportunityEntity Create(int venueId, DateRange period, int contractId, IEnumerable<int>? genreIds = null)
    {
        var opportunity = new OpportunityEntity
        {
            VenueId = venueId,
            Period = period,
            ContractId = contractId
        };

        if (genreIds is not null)
            opportunity.SyncGenres(genreIds);

        return opportunity;
    }

    public void Update(DateRange period, int contractId, IEnumerable<int> genreIds)
    {
        Period = period;
        ContractId = contractId;
        SyncGenres(genreIds);
    }

    public void SyncGenres(IEnumerable<int> genreIds) =>
        this.SyncGenres<OpportunityGenreEntity>(genreIds);
}
