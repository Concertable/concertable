using Concertable.Shared.Exceptions;
using Concertable.Application.Interfaces;
using FluentResults;

namespace Concertable.Infrastructure.Services.Concert;

internal class ConcertDraftService : IConcertDraftService
{
    private readonly IConcertBookingRepository bookingRepository;
    private readonly IConcertNotificationService concertNotificationService;

    public ConcertDraftService(
        IConcertBookingRepository bookingRepository,
        IConcertNotificationService concertNotificationService)
    {
        this.bookingRepository = bookingRepository;
        this.concertNotificationService = concertNotificationService;
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
            $"{artist.Name} performing at {venue.Name}",
            venue.About,
            matchingGenreIds);

        bookingConcert.Confirm(concert);
        await bookingRepository.SaveChangesAsync();

        await concertNotificationService.ConcertDraftCreatedAsync(artist.UserId.ToString(), concert.Id);

        return Result.Ok(concert);
    }
}
