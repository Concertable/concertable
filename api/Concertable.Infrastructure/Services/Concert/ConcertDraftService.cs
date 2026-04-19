using Concertable.Application.Exceptions;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities;
using FluentResults;

namespace Concertable.Infrastructure.Services.Concert;

public class ConcertDraftService : IConcertDraftService
{
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly IOpportunityRepository opportunityRepository;
    private readonly IApplicationAcceptHandler acceptHandler;
    private readonly IConcertNotificationService concertNotificationService;
    private readonly IApplicationNotificationService applicationNotificationService;
    private readonly IManagerRepository<ArtistManagerEntity> artistManagerRepository;

    public ConcertDraftService(
        IOpportunityApplicationRepository applicationRepository,
        IOpportunityRepository opportunityRepository,
        IApplicationAcceptHandler acceptHandler,
        IConcertNotificationService concertNotificationService,
        IApplicationNotificationService applicationNotificationService,
        IManagerRepository<ArtistManagerEntity> artistManagerRepository)
    {
        this.applicationRepository = applicationRepository;
        this.opportunityRepository = opportunityRepository;
        this.acceptHandler = acceptHandler;
        this.concertNotificationService = concertNotificationService;
        this.applicationNotificationService = applicationNotificationService;
        this.artistManagerRepository = artistManagerRepository;
    }

    public async Task<Result<ConcertEntity>> CreateAsync(int applicationId)
    {
        var (artist, venue) = await applicationRepository.GetArtistAndVenueByIdAsync(applicationId)
            ?? throw new NotFoundException("Concert application not found");

        var opportunity = await opportunityRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Opportunity not found");

        var artistGenreIds = artist.ArtistGenres.Select(ag => ag.GenreId);
        var opportunityGenreIds = opportunity.OpportunityGenres.Select(og => og.GenreId);

        var matchingGenreIds = opportunityGenreIds.Any()
            ? artistGenreIds.Intersect(opportunityGenreIds)
            : artistGenreIds;

        if (!matchingGenreIds.Any())
            return Result.Fail("The artist does not match any genres required by the concert opportunity");

        var concert = ConcertEntity.CreateDraft(
            applicationId,
            $"{artist.Name} performing at {venue.Name}",
            venue.About,
            matchingGenreIds);

        await acceptHandler.HandleAsync(applicationId, concert);

        var artistManager = await artistManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Artist manager not found");

        await concertNotificationService.ConcertDraftCreatedAsync(artist.UserId.ToString(), concert.Id);
        await applicationNotificationService.ApplicationAcceptedAsync(artistManager.Id.ToString(), concert.Id);

        return Result.Ok(concert);
    }
}
