
using Concertable.Application.DTOs;
using Concertable.Application.Exceptions;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Application.Interfaces.Search;
using Concertable.Application.Mappers;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Concertable.Core.Parameters;
using FluentResults;

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
    private readonly IGeometryCalculator geometryCalculator;
    private readonly IConcertDraftService concertDraftService;
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
        IConcertDraftService concertDraftService,
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
        this.preferenceService = preferenceService;
        this.geometryCalculator = geometryCalculator;
        this.concertDraftService = concertDraftService;
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

    public Task<Result<ConcertEntity>> CreateDraftAsync(int applicationId) =>
        concertDraftService.CreateAsync(applicationId);

    public async Task<ConcertDto> GetDetailsByApplicationIdAsync(int applicationId)
    {
        return await concertRepository.GetDtoByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException($"No concert found for Application ID {applicationId}");
    }

    public async Task<ConcertUpdateResponse> UpdateAsync(int id, UpdateConcertRequest request)
    {
        var concertEntity = await concertRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Concert not found");

        var result = await concertValidator.CanUpdateAsync(concertEntity, request.TotalTickets);
        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        concertEntity.Update(request.Name, request.About, request.Price, request.TotalTickets);

        await concertRepository.SaveChangesAsync();

        return new ConcertUpdateResponse
        {
            Id = concertEntity.Id,
            Name = concertEntity.Name,
            About = concertEntity.About,
            Price = concertEntity.Price,
            TotalTickets = concertEntity.TotalTickets,
            AvailableTickets = concertEntity.AvailableTickets
        };
    }

    public async Task<ConcertPostResponse> PostAsync(int id, UpdateConcertRequest request)
    {
        var concertEntity = await concertRepository.GetFullByIdAsync(id)
            ?? throw new NotFoundException("Concert not found");

        var result = await concertValidator.CanPostAsync(concertEntity);
        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        concertEntity.Post(request.Name, request.About, request.Price, request.TotalTickets, timeProvider.GetUtcNow().DateTime);

        await concertRepository.SaveChangesAsync();

        var concertHeaderDto = concertEntity.ToHeaderDto();
        concertHeaderDto.Rating = (await concertReviewRepository.GetSummaryAsync(id)).AverageRating;

        var location = concertEntity.Application.Opportunity.Venue.User.Location;

        if (location is null)
            return new ConcertPostResponse { ConcertHeader = concertHeaderDto, UserIds = [] };

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

        return new ConcertPostResponse { ConcertHeader = concertHeaderDto, UserIds = userIdsToNotify };
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
