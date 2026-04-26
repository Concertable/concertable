using Concertable.Shared.Exceptions;
using FluentResults;

namespace Concertable.Concert.Infrastructure.Services;

internal class ConcertDraftService : IConcertDraftService
{
    private readonly IConcertBookingRepository bookingRepository;
    private readonly IConcertNotifier notifier;

    public ConcertDraftService(
        IConcertBookingRepository bookingRepository,
        IConcertNotifier notifier)
    {
        this.bookingRepository = bookingRepository;
        this.notifier = notifier;
    }

    public async Task<Result<ConcertEntity>> CreateAsync(int bookingId)
    {
        var bookingConcert = await bookingRepository.GetByIdAsync(bookingId)
            ?? throw new NotFoundException("Booking not found");

        var artist = bookingConcert.Application.Artist;
        var opportunity = bookingConcert.Application.Opportunity;
        var venue = opportunity.Venue;

        var artistGenreIds = artist.Genres.Select(g => g.GenreId);
        var opportunityGenreIds = opportunity.OpportunityGenres.Select(og => og.GenreId);

        var matchingGenreIds = opportunityGenreIds.Any()
            ? artistGenreIds.Intersect(opportunityGenreIds)
            : artistGenreIds;

        if (!matchingGenreIds.Any())
            return Result.Fail("The artist does not match any genres required by the concert opportunity");

        var concert = ConcertEntity.CreateDraft(
            bookingConcert.Id,
            artist.Id,
            venue.Id,
            opportunity.Period.Start,
            opportunity.Period.End,
            $"{artist.Name} performing at {venue.Name}",
            venue.About,
            matchingGenreIds);

        bookingConcert.Confirm(concert);
        await bookingRepository.SaveChangesAsync();

        await notifier.ConcertDraftCreatedAsync(artist.UserId.ToString(), concert.Id);

        return Result.Ok(concert);
    }
}
