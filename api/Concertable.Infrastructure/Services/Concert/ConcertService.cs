using Core.Entities;
using Core.Enums;
using Application.Interfaces;
using Application.Interfaces.Concert;
using Application.Interfaces.Geometry;
using Application.Interfaces.Search;
using Core.Parameters;
using Application.DTOs;
using Application.Mappers;
using Application.Requests;
using Application.Responses;
using Core.Exceptions;

namespace Infrastructure.Services.Concert;

public class ConcertService : IConcertService
{
    private readonly IConcertRepository concertRepository;
    private readonly IConcertHeaderService concertHeaderService;
    private readonly IConcertValidator concertValidator;
    private readonly ICurrentUser currentUser;
    private readonly IConcertApplicationValidator applicationValidator;
    private readonly IMessageService messageService;
    private readonly IEmailService emailService;
    private readonly IReviewService reviewService;
    private readonly IPreferenceService preferenceService;
    private readonly IConcertOpportunityRepository opportunityRepository;
    private readonly IGeometryCalculator geometryCalculator;
    private readonly IConcertApplicationRepository applicationRepository;
    private readonly IGenreRepository genreRepository;
    private readonly TimeProvider timeProvider;

    public ConcertService(
        IConcertRepository concertRepository,
        IConcertHeaderService concertHeaderService,
        IConcertValidator concertValidator,
        ICurrentUser currentUser,
        IConcertApplicationValidator applicationValidator,
        IMessageService messageService,
        IEmailService emailService,
        IReviewService reviewService,
        IPreferenceService preferenceService,
        IGeometryCalculator geometryCalculator,
        IConcertOpportunityRepository opportunityRepository,
        IConcertApplicationRepository applicationRepository,
        IGenreRepository genreRepository,
        TimeProvider timeProvider)
    {
        this.concertRepository = concertRepository;
        this.concertHeaderService = concertHeaderService;
        this.concertValidator = concertValidator;
        this.currentUser = currentUser;
        this.applicationValidator = applicationValidator;
        this.messageService = messageService;
        this.emailService = emailService;
        this.reviewService = reviewService;
        this.applicationRepository = applicationRepository;
        this.preferenceService = preferenceService;
        this.opportunityRepository = opportunityRepository;
        this.geometryCalculator = geometryCalculator;
        this.genreRepository = genreRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<IEnumerable<ConcertDto>> GetUpcomingByVenueIdAsync(int id)
    {
        var concerts = await concertRepository.GetUpcomingByVenueIdAsync(id);
        return concerts.ToDtos();
    }

    public async Task<IEnumerable<ConcertDto>> GetUpcomingByArtistIdAsync(int id)
    {
        var concerts = await concertRepository.GetUpcomingByArtistIdAsync(id);
        return concerts.ToDtos();
    }

    public async Task<IEnumerable<ConcertDto>> GetHistoryByArtistIdAsync(int id)
    {
        var concerts = await concertRepository.GetHistoryByArtistIdAsync(id);
        return concerts.ToDtos();
    }

    public async Task<IEnumerable<ConcertDto>> GetHistoryByVenueIdAsync(int id)
    {
        var concerts = await concertRepository.GetHistoryByVenueIdAsync(id);
        return concerts.ToDtos();
    }

    public async Task<ConcertDto> GetDetailsByIdAsync(int id)
    {
        var concertEntity = await concertRepository.GetDetailsByIdAsync(id)
            ?? throw new NotFoundException("Concert not found");
        return concertEntity.ToDto();
    }

    public async Task<ConcertApplicationPurchaseResponse> CompleteAsync(PurchaseCompleteDto purchaseCompleteDto)
    {
        try
        {
            var (artist, venue) = await applicationRepository.GetArtistAndVenueByIdAsync(purchaseCompleteDto.EntityId)
                ?? throw new NotFoundException("Concert application not found");
            var opportunity = await opportunityRepository.GetByApplicationIdAsync(purchaseCompleteDto.EntityId);
            var concertDto = await CreateDefaultAsync(purchaseCompleteDto, artist, opportunity!);

            await messageService.SendAndSaveAsync(
                purchaseCompleteDto.FromUserId,
                purchaseCompleteDto.ToUserId,
                "concert",
                concertDto.Id,
                "Your Application has been accepted! View your concert here");

            await emailService.SendEmailAsync(artist.User.Email!, "Concert Creation", "Your Application was chosen! A Concert has been scheduled for you!");

            return new ConcertApplicationPurchaseResponse
            {
                Success = true,
                Message = "Concert successfully booked!",
                ApplicationId = purchaseCompleteDto.EntityId,
                Concert = concertDto
            };
        }
        catch (Exception)
        {
            return new ConcertApplicationPurchaseResponse
            {
                Success = false,
                Message = "An error occurred while completing your booking. Please contact support.",
                ApplicationId = purchaseCompleteDto.EntityId
            };
        }
    }

    public async Task<ConcertDto> CreateDefaultAsync(PurchaseCompleteDto purchaseCompleteDto, ArtistEntity artist, ConcertOpportunityEntity opportunity)
    {
        var artistGenreIds = artist.ArtistGenres.Select(ag => ag.GenreId);
        var opportunityGenreIds = opportunity.OpportunityGenres.Select(og => og.GenreId);

        var matchingGenreIds = opportunityGenreIds.Any()
            ? artistGenreIds.Intersect(opportunityGenreIds)
            : artistGenreIds;

        if (!matchingGenreIds.Any())
            throw new BadRequestException("The artist does not match any genres required by the concert opportunity");

        var matchingGenres = await genreRepository.GetByIdsAsync(matchingGenreIds);

        var concertEntity = new Core.Entities.ConcertEntity
        {
            ApplicationId = purchaseCompleteDto.EntityId,
            Name = $"{artist.Name} performing at {opportunity.Venue.Name}",
            About = opportunity.Venue.About,
            Price = 0,
            TotalTickets = 0,
            AvailableTickets = 0,
            DatePosted = null,
            ConcertGenres = matchingGenres
                .Select(g => new ConcertGenreEntity { GenreId = g.Id, Genre = g })
                .ToList()
        };

        await concertRepository.AddAsync(concertEntity);
        await concertRepository.SaveChangesAsync();

        return concertEntity.ToDto();
    }

    public async Task<ConcertDto> GetDetailsByApplicationIdAsync(int applicationId)
    {
        var concertEntity = await concertRepository.GetByApplicationIdAsync(applicationId);

        if (concertEntity is null)
            throw new NotFoundException($"No concert found for Application ID {applicationId}");

        return concertEntity.ToDto();
    }

    public async Task<ConcertDto> UpdateAsync(int id, UpdateConcertRequest request)
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

        return concertEntity.ToDto();
    }

    public async Task<ConcertPostResponse> PostAsync(int id, UpdateConcertRequest request)
    {
        var concertEntity = await concertRepository.GetDetailsByIdAsync(id)
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

        var concertHeaderDto = concertEntity.ToDto().ToHeaderDto();
        var averageRating = (await reviewService.GetSummaryByConcertIdAsync(id)).AverageRating;
        concertHeaderDto.Rating = averageRating;

        var location = concertEntity.Application.Opportunity.Venue.User.Location;

        if (location == null || location?.Y == null || location?.X == null)
        {
            return new ConcertPostResponse
            {
                Concert = concertEntity.ToDto(),
                ConcertHeader = concertHeaderDto,
                UserIds = Enumerable.Empty<int>()
            };
        }

        var preferences = await preferenceService.GetAsync();
        var concertDto = concertEntity.ToDto();

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

                var hasMatchingGenre = preference.Genres.Any(userGenre =>
                    concertDto.Genres.Any(concertGenre => concertGenre.Id == userGenre.Id));

                return inRange && hasMatchingGenre;
            })
            .Select(preference => preference.User.Id)
            .ToList();

        return new ConcertPostResponse
        {
            Concert = concertEntity.ToDto(),
            ConcertHeader = concertHeaderDto,
            UserIds = userIdsToNotify
        };
    }

    public async Task<IEnumerable<ConcertHeaderDto>> GetRecommendedHeadersAsync()
    {
        var user = currentUser.GetOrDefault();

        if (user is null)
            return Enumerable.Empty<ConcertHeaderDto>();

        var preferences = await preferenceService.GetByUserIdAsync(user.Id);

        if (preferences is null)
            return Enumerable.Empty<ConcertHeaderDto>();

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

    public async Task<IEnumerable<ConcertDto>> GetUnpostedByArtistIdAsync(int id)
    {
        var concerts = await concertRepository.GetUnpostedByArtistIdAsync(id);
        return concerts.ToDtos();
    }

    public async Task<IEnumerable<ConcertDto>> GetUnpostedByVenueIdAsync(int id)
    {
        var concerts = await concertRepository.GetUnpostedByVenueIdAsync(id);
        return concerts.ToDtos();
    }

}
