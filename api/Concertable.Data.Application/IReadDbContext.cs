using Concertable.Artist.Domain;
using Concertable.Core.Entities;
using Concertable.Identity.Domain;
using Concertable.Shared;
using Concertable.Venue.Domain;

namespace Concertable.Data.Application;

public interface IReadDbContext
{
    IQueryable<UserEntity> Users { get; }
    IQueryable<RefreshTokenEntity> RefreshTokens { get; }
    IQueryable<EmailVerificationTokenEntity> EmailVerificationTokens { get; }
    IQueryable<PasswordResetTokenEntity> PasswordResetTokens { get; }
    IQueryable<ArtistEntity> Artists { get; }
    IQueryable<ArtistGenreEntity> ArtistGenres { get; }
    IQueryable<VenueEntity> Venues { get; }
    IQueryable<VenueImageEntity> VenueImages { get; }
    IQueryable<ConcertEntity> Concerts { get; }
    IQueryable<ConcertGenreEntity> ConcertGenres { get; }
    IQueryable<ConcertImageEntity> ConcertImages { get; }
    IQueryable<GenreEntity> Genres { get; }
    IQueryable<OpportunityEntity> Opportunities { get; }
    IQueryable<OpportunityGenreEntity> OpportunityGenres { get; }
    IQueryable<OpportunityApplicationEntity> OpportunityApplications { get; }
    IQueryable<ConcertBookingEntity> ConcertBookings { get; }
    IQueryable<ReviewEntity> Reviews { get; }
    IQueryable<TicketEntity> Tickets { get; }
    IQueryable<MessageEntity> Messages { get; }
    IQueryable<TransactionEntity> Transactions { get; }
    IQueryable<TicketTransactionEntity> TicketTransactions { get; }
    IQueryable<SettlementTransactionEntity> SettlementTransactions { get; }
    IQueryable<PreferenceEntity> Preferences { get; }
    IQueryable<GenrePreferenceEntity> GenrePreferences { get; }
    IQueryable<StripeEventEntity> StripeEvents { get; }
    IQueryable<ContractEntity> Contracts { get; }
    IQueryable<FlatFeeContractEntity> FlatFeeContracts { get; }
    IQueryable<DoorSplitContractEntity> DoorSplitContracts { get; }
    IQueryable<VersusContractEntity> VersusContracts { get; }
    IQueryable<VenueHireContractEntity> VenueHireContracts { get; }
}
