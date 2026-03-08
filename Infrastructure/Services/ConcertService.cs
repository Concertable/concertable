using Core.Entities;
using Application.Interfaces;
using Core.Parameters;
using Application.DTOs;
using Application.Mappers;
using Application.Responses;
using Core.Exceptions;

namespace Infrastructure.Services;

public class ConcertService : IConcertService
{
    private readonly IConcertRepository concertRepository;
    private readonly IConcertValidationService concertValidationService;
    private readonly ICurrentUser currentUser;
    private readonly IUserPaymentService userPaymentService;
    private readonly IListingApplicationValidationService applicationValidationService;
    private readonly IMessageService messageService;
    private readonly IEmailService emailService;
    private readonly IReviewService reviewService;
    private readonly IPreferenceService preferenceService;
    private readonly IListingRepository listingRepository;
    private readonly ILocationService locationService;
    private readonly IListingApplicationRepository listingApplicationRepository;
    private readonly IGenreRepository genreRepository;
    private readonly TimeProvider timeProvider;

    public ConcertService(
        IConcertRepository concertRepository,
        IConcertValidationService concertValidationService,
        ICurrentUser currentUser,
        IUserPaymentService userPaymentService,
        IListingApplicationValidationService applicationValidationService,
        IMessageService messageService,
        IEmailService emailService,
        IReviewService reviewService,
        IPreferenceService preferenceService,
        ILocationService locationService,
        IListingRepository listingRepository,
        IListingApplicationRepository listingApplicationRepository,
        IGenreRepository genreRepository,
        TimeProvider timeProvider)
    {
        this.concertRepository = concertRepository;
        this.concertValidationService = concertValidationService;
        this.currentUser = currentUser;
        this.userPaymentService = userPaymentService;
        this.applicationValidationService = applicationValidationService;
        this.messageService = messageService;
        this.emailService = emailService;
        this.reviewService = reviewService;
        this.listingApplicationRepository = listingApplicationRepository;
        this.preferenceService = preferenceService;
        this.listingRepository = listingRepository;
        this.locationService = locationService;
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
        var concertEntity = await concertRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Concert not found");
        return concertEntity.ToDto();
    }

    public async Task<ListingApplicationPurchaseResponse> BookAsync(ConcertBookingParams bookingParams)
    {
        var user = currentUser.Get();
        var role = currentUser.GetFirstRole();

        if (role != "VenueManager")
            throw new ForbiddenException("Only VenueManagers can book concerts");

        var response = await applicationValidationService.CanAcceptListingApplicationAsync(bookingParams.ApplicationId, user.Id);

        if (!response.IsValid)
            throw new BadRequestException(response.Reason!);

        var paymentResponse = await userPaymentService.PayArtistManagerByApplicationIdAsync(bookingParams.ApplicationId, bookingParams.PaymentMethodId);

        return new ListingApplicationPurchaseResponse
        {
            Success = paymentResponse.Success,
            RequiresAction = paymentResponse.RequiresAction,
            Message = paymentResponse.Message ?? (paymentResponse.Success ? "Payment successful" : "Payment failed"),
            ApplicationId = bookingParams.ApplicationId,
            TransactionId = paymentResponse.TransactionId,
            UserEmail = user.Email,
            ClientSecret = paymentResponse.ClientSecret
        };
    }

    public async Task<ListingApplicationPurchaseResponse> CompleteAsync(PurchaseCompleteDto purchaseCompleteDto)
    {
        try
        {
            var (artist, venue) = await listingApplicationRepository.GetArtistAndVenueByIdAsync(purchaseCompleteDto.EntityId);
            var listing = await listingRepository.GetByApplicationIdAsync(purchaseCompleteDto.EntityId);
            var concertDto = await CreateDefaultAsync(purchaseCompleteDto, artist, listing!);

            await messageService.SendAndSaveAsync(
                purchaseCompleteDto.FromUserId,
                purchaseCompleteDto.ToUserId,
                "concert",
                concertDto.Id,
                "Your Application has been accepted! View your concert here");

            await emailService.SendEmailAsync(artist.User.Email!, "Concert Creation", "Your Application was chosen! A Concert has been scheduled for you!");

            return new ListingApplicationPurchaseResponse
            {
                Success = true,
                Message = "Concert successfully booked!",
                ApplicationId = purchaseCompleteDto.EntityId,
                Concert = concertDto
            };
        }
        catch (Exception)
        {
            return new ListingApplicationPurchaseResponse
            {
                Success = false,
                Message = "An error occurred while completing your booking. Please contact support.",
                ApplicationId = purchaseCompleteDto.EntityId
            };
        }
    }

    public async Task<ConcertDto> CreateDefaultAsync(PurchaseCompleteDto purchaseCompleteDto, Artist artist, Listing listing)
    {
        var artistGenreIds = artist.ArtistGenres.Select(ag => ag.GenreId);
        var listingGenreIds = listing.ListingGenres.Select(lg => lg.GenreId);

        var matchingGenreIds = listingGenreIds.Any()
            ? artistGenreIds.Intersect(listingGenreIds)
            : artistGenreIds;

        if (!matchingGenreIds.Any())
            throw new BadRequestException("The artist does not match any genres required by the listing");

        var matchingGenres = await genreRepository.GetByIdsAsync(matchingGenreIds);

        var concertEntity = new Concert
        {
            ApplicationId = purchaseCompleteDto.EntityId,
            Name = $"{artist.Name} performing at {listing.Venue.Name}",
            About = listing.Venue.About,
            Price = 0,
            TotalTickets = 0,
            AvailableTickets = 0,
            DatePosted = null,
            ConcertGenres = matchingGenres
                .Select(g => new ConcertGenre { GenreId = g.Id, Genre = g })
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

    public async Task<ConcertDto> UpdateAsync(ConcertDto concertDto)
    {
        var response = await concertValidationService.CanUpdateAsync(concertDto);
        if (!response.IsValid)
            throw new BadRequestException(response.Reason!);

        var concertEntity = await concertRepository.GetByIdAsync(concertDto.Id);
        if (concertEntity is null)
            throw new NotFoundException("Concert not found");

        concertEntity.Name = concertDto.Name;
        concertEntity.About = concertDto.About;
        concertEntity.Price = concertDto.Price;
        concertEntity.TotalTickets = concertDto.TotalTickets;
        concertEntity.AvailableTickets = concertDto.AvailableTickets;

        int ticketsSold = concertEntity.TotalTickets - concertEntity.AvailableTickets;
        concertEntity.AvailableTickets = concertEntity.TotalTickets - ticketsSold;

        concertRepository.Update(concertEntity);
        await concertRepository.SaveChangesAsync();

        return concertEntity.ToDto();
    }

    public async Task<ConcertPostResponse> PostAsync(ConcertDto concertDto)
    {
        var response = await concertValidationService.CanPostAsync(concertDto);
        if (!response.IsValid)
            throw new BadRequestException(response.Reason!);

        var concertEntity = await concertRepository.GetByIdAsync(concertDto.Id);

        if (concertEntity is null)
            throw new NotFoundException("Concert not found");

        if (concertEntity.DatePosted.HasValue)
            throw new BadRequestException("Concert has already been posted");

        concertEntity.Name = concertDto.Name;
        concertEntity.About = concertDto.About;
        concertEntity.Price = concertDto.Price;
        concertEntity.TotalTickets = concertDto.TotalTickets;
        concertEntity.DatePosted = timeProvider.GetUtcNow().DateTime;
        concertEntity.AvailableTickets = concertDto.TotalTickets;

        concertRepository.Update(concertEntity);
        await concertRepository.SaveChangesAsync();

        var concertHeaderDto = concertDto.ToHeaderDto();
        var averageRating = (await reviewService.GetSummaryByConcertIdAsync(concertDto.Id)).AverageRating;
        concertHeaderDto.Rating = averageRating;

        var location = concertEntity.Application.Listing.Venue.User.Location;

        if (location == null || location?.Y == null || location?.X == null)
        {
            return new ConcertPostResponse
            {
                Concert = concertDto,
                ConcertHeader = concertHeaderDto,
                UserIds = Enumerable.Empty<int>()
            };
        }

        var preferences = await preferenceService.GetAsync();

        var userIdsToNotify = preferences
            .Where(preference =>
            {
                var inRange = locationService.IsWithinRadius(
                    preference.User.Latitude,
                    preference.User.Longitude,
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

        var result = await concertRepository.GetHeaders(user.Id, concertParams);
        await reviewService.AddAverageRatingsAsync(result);
        return result.Take(concertParams.Take);
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
