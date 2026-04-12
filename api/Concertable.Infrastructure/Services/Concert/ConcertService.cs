
using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Application.Interfaces.Search;
using Concertable.Application.Mappers;
using Concertable.Application.Requests;
using Concertable.Application.Results;
using Concertable.Core.Entities;
using Concertable.Core.Exceptions;
using Concertable.Core.Parameters;
using Org.BouncyCastle.Asn1.Cmp;

namespace Concertable.Infrastructure.Services.Concert;

public class ConcertService : IConcertService
{
    private readonly IConcertRepository concertRepository;
    private readonly IConcertHeaderService concertHeaderService;
    private readonly IConcertValidator concertValidator;
    private readonly ICurrentUser currentUser;
    private readonly IOpportunityApplicationValidator applicationValidator;
    private readonly IMessageService messageService;
    private readonly IEmailService emailService;
    private readonly IConcertReviewRepository concertReviewRepository;
    private readonly IPreferenceService preferenceService;
    private readonly IOpportunityRepository opportunityRepository;
    private readonly IGeometryCalculator geometryCalculator;
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly IGenreRepository genreRepository;
    private readonly IConcertNotificationService concertNotificationService;
    private readonly TimeProvider timeProvider;

    public ConcertService(
        IConcertRepository concertRepository,
        IConcertHeaderService concertHeaderService,
        IConcertValidator concertValidator,
        ICurrentUser currentUser,
        IOpportunityApplicationValidator applicationValidator,
        IMessageService messageService,
        IEmailService emailService,
        IConcertReviewRepository concertReviewRepository,
        IPreferenceService preferenceService,
        IGeometryCalculator geometryCalculator,
        IOpportunityRepository opportunityRepository,
        IOpportunityApplicationRepository applicationRepository,
        IGenreRepository genreRepository,
        IConcertNotificationService concertNotificationService,
        TimeProvider timeProvider)
    {
        this.concertRepository = concertRepository;
        this.concertHeaderService = concertHeaderService;
        this.concertValidator = concertValidator;
        this.currentUser = currentUser;
        this.applicationValidator = applicationValidator;
        this.messageService = messageService;
        this.emailService = emailService;
        this.concertReviewRepository = concertReviewRepository;
        this.applicationRepository = applicationRepository;
        this.preferenceService = preferenceService;
        this.opportunityRepository = opportunityRepository;
        this.geometryCalculator = geometryCalculator;
        this.genreRepository = genreRepository;
        this.concertNotificationService = concertNotificationService;
        this.timeProvider = timeProvider;
    }

    public Task<IEnumerable<ConcertSummaryDto>> GetUpcomingByVenueIdAsync(int id) =>
        concertRepository.GetUpcomingByVenueIdAsync(id);

    public Task<IEnumerable<ConcertSummaryDto>> GetUpcomingByArtistIdAsync(int id) =>
        concertRepository.GetUpcomingByArtistIdAsync(id);

    public Task<IEnumerable<ConcertSummaryDto>> GetHistoryByArtistIdAsync(int id) =>
        concertRepository.GetHistoryByArtistIdAsync(id);

    public Task<IEnumerable<ConcertSummaryDto>> GetHistoryByVenueIdAsync(int id) =>
        concertRepository.GetHistoryByVenueIdAsync(id);

    public async Task<ConcertDto> GetDetailsByIdAsync(int id)
    {
        return await concertRepository.GetDtoByIdAsync(id)
            ?? throw new NotFoundException("Concert not found");
    }

    public async Task<int> CreateDraftAsync(int applicationId)
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
            throw new BadRequestException("The artist does not match any genres required by the concert opportunity");

        var matchingGenres = await genreRepository.GetByIdsAsync(matchingGenreIds);

        var concertEntity = new ConcertEntity
        {
            ApplicationId = applicationId,
            Name = $"{artist.Name} performing at {venue.Name}",
            About = venue.About,
            Price = 0,
            TotalTickets = 0,
            AvailableTickets = 0,
            DatePosted = null,
            ConcertGenres = matchingGenres
                .Select(g => new ConcertGenreEntity { GenreId = g.Id })
                .ToList()
        };

        await concertRepository.AddAsync(concertEntity);
        await concertRepository.SaveChangesAsync();

        await concertNotificationService.ConcertDraftCreatedAsync(artist.UserId.ToString(), concertEntity.Id);

        return concertEntity.Id;
    }

    public async Task<ConcertDto> GetDetailsByApplicationIdAsync(int applicationId)
    {
        return await concertRepository.GetDtoByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException($"No concert found for Application ID {applicationId}");
    }

    public async Task<ConcertUpdateResult> UpdateAsync(int id, UpdateConcertRequest request)
    {
        var concertEntity = await concertRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Concert not found");

        var result = await concertValidator.CanUpdateAsync(concertEntity, request.TotalTickets);
        if (!result.IsValid)
            throw new BadRequestException(result.Errors);

        concertEntity.Name = request.Name;
        concertEntity.About = request.About;
        concertEntity.Price = request.Price;
        concertEntity.TotalTickets = request.TotalTickets;

        int ticketsSold = concertEntity.TotalTickets - concertEntity.AvailableTickets;
        concertEntity.AvailableTickets = request.TotalTickets - ticketsSold;

        await concertRepository.SaveChangesAsync();

        return new ConcertUpdateResult
        {
            Id = concertEntity.Id,
            Name = concertEntity.Name,
            About = concertEntity.About,
            Price = concertEntity.Price,
            TotalTickets = concertEntity.TotalTickets,
            AvailableTickets = concertEntity.AvailableTickets
        };
    }

    public async Task<ConcertPostResult> PostAsync(int id, UpdateConcertRequest request)
    {
        var concertEntity = await concertRepository.GetFullByIdAsync(id)
            ?? throw new NotFoundException("Concert not found");

        var result = await concertValidator.CanPostAsync(concertEntity);
        if (!result.IsValid)
            throw new BadRequestException(result.Errors);

        concertEntity.Name = request.Name;
        concertEntity.About = request.About;
        concertEntity.Price = request.Price;
        concertEntity.TotalTickets = request.TotalTickets;
        concertEntity.DatePosted = timeProvider.GetUtcNow().DateTime;
        concertEntity.AvailableTickets = request.TotalTickets;

        await concertRepository.SaveChangesAsync();

        var concertHeaderDto = concertEntity.ToHeaderDto();
        concertHeaderDto.Rating = (await concertReviewRepository.GetSummaryAsync(id)).AverageRating;

        var location = concertEntity.Application.Opportunity.Venue.User.Location;

        if (location is null)
            return new ConcertPostResult { ConcertHeader = concertHeaderDto, UserIds = [] };

        var preferences = await preferenceService.GetAsync();
        var genreIds = concertEntity.ConcertGenres.Select(cg => cg.GenreId).ToHashSet();

        var userIdsToNotify = preferences
            .Where(preference =>
            {
                if (preference.User.Latitude is null || preference.User.Longitude is null)
                    return false;

                var inRange = geometryCalculator.IsWithinRadius(
                    preference.User.Latitude.Value,
                    preference.User.Longitude.Value,
                    location.Y,
                    location.X,
                    preference.RadiusKm);

                var hasMatchingGenre = preference.Genres.Any(g => genreIds.Contains(g.Id));

                return inRange && hasMatchingGenre;
            })
            .Select(preference => preference.User.Id)
            .ToList();

        return new ConcertPostResult { ConcertHeader = concertHeaderDto, UserIds = userIdsToNotify };
    }

    public async Task<IEnumerable<ConcertHeaderDto>> GetRecommendedHeadersAsync()
    {
        var user = currentUser.GetOrDefault();

        if (user is null)
            return [];

        var preferences = await preferenceService.GetByUserIdAsync(user.Id);

        if (preferences is null)
            return [];

        var concertParams = new ConcertParams
        {
            Latitude = user.Latitude,
            Longitude = user.Longitude,
            RadiusKm = preferences.RadiusKm,
            GenreIds = preferences.Genres.Select(g => g.Id).ToList(),
            OrderByRecent = true,
            Take = 10
        };

        return await concertHeaderService.GetRecommendedAsync(concertParams);
    }

    public Task<IEnumerable<ConcertSummaryDto>> GetUnpostedByArtistIdAsync(int id) =>
        concertRepository.GetUnpostedByArtistIdAsync(id);

    public Task<IEnumerable<ConcertSummaryDto>> GetUnpostedByVenueIdAsync(int id) =>
        concertRepository.GetUnpostedByVenueIdAsync(id);

}
