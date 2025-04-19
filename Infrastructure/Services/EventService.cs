using AutoMapper;
using Core.Entities;
using Application.Interfaces;
using Core.Parameters;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Responses;
using System.Diagnostics;
using Application.Responses;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Core.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Stripe.Terminal;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Services
{
    public class EventService : HeaderService<Event, EventHeaderDto, IEventRepository>, IEventService
    {
        private readonly IEventRepository eventRepository;
        private readonly ICurrentUserService currentUserService;
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
        private readonly IMapper mapper;

        public EventService(
            IEventRepository eventRepository, 
            ICurrentUserService currentUserService,
            IUserPaymentService userPaymentService,
            IListingApplicationValidationService applicationValidationService,
            IMessageService messageService,
            IEmailService emailService,
            IReviewService reviewService, 
            IGeometryService geometryService,
            IPreferenceService preferenceService,
            ILocationService locationService,
            IListingRepository listingRepository,
            IListingApplicationRepository listingApplicationRepository,
            IGenreRepository genreRepository,
            IMapper mapper) : base(eventRepository, geometryService)
        {
            this.eventRepository = eventRepository;
            this.currentUserService = currentUserService;
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
            this.mapper = mapper;
        }

        public async Task<IEnumerable<EventDto>> GetUpcomingByVenueIdAsync(int id)
        {
            var events = await eventRepository.GetUpcomingByVenueIdAsync(id);
            return mapper.Map<IEnumerable<EventDto>>(events);
        }

        public async override Task<PaginationResponse<EventHeaderDto>> GetHeadersAsync(SearchParams? searchParams)
        {
            var headers = await base.GetHeadersAsync(searchParams);

            await reviewService.AddAverageRatingsAsync(headers.Data);

            return headers;
        }

        public async Task<IEnumerable<EventDto>> GetUpcomingByArtistIdAsync(int id)
        {
            var events = await eventRepository.GetUpcomingByArtistIdAsync(id);
            return mapper.Map<IEnumerable<EventDto>>(events);
        }

        public async Task<IEnumerable<EventDto>> GetHistoryByArtistIdAsync(int id)
        {
            var events = await eventRepository.GetHistoryByVenueIdAsync(id);
            return mapper.Map<IEnumerable<EventDto>>(events);
        }

        public async Task<IEnumerable<EventDto>> GetHistoryByVenueIdAsync(int id)
        {
            var events = await eventRepository.GetHistoryByVenueIdAsync(id);
            return mapper.Map<IEnumerable<EventDto>>(events);
        }

        public async Task<EventDto> GetDetailsByIdAsync(int id)
        {
            var eventEntity = await eventRepository.GetByIdAsync(id);

            return mapper.Map<EventDto>(eventEntity);
        }

        public async Task<ListingApplicationPurchaseResponse> BookAsync(EventBookingParams bookingParams)
        {
            var user = await currentUserService.GetAsync();
            var role = await currentUserService.GetFirstRoleAsync();

            if (role != "VenueManager")
                throw new ForbiddenException("Only VenueManagers can book events");

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

                var eventDto = await CreateDefaultAsync(purchaseCompleteDto, artist, listing!);

                await messageService.SendAsync(
                    purchaseCompleteDto.FromUserId,
                    purchaseCompleteDto.ToUserId,
                    "event",
                    eventDto.Id,
                    "Your Application has been accepted! View your event here"
                );

                await emailService.SendEmailAsync(purchaseCompleteDto.FromUserId, purchaseCompleteDto.FromEmail, "Event Creation", "Your Event has been Created!");
                //await emailService.SendEmailAsync(purchaseCompleteDto.ToUserId, purchaseCompleteDto.ToEmail, "Event Creation", "An Event has been schedueled for you!");

                return new ListingApplicationPurchaseResponse
                {
                    Success = true,
                    Message = "Event successfully booked!",
                    ApplicationId = purchaseCompleteDto.EntityId,
                    Event = eventDto
                };
            } catch (Exception ex)
            {
                return new ListingApplicationPurchaseResponse
                {
                    Success = false,
                    Message = "An error occurred while completing your booking. Please contact support.",
                    ApplicationId = purchaseCompleteDto.EntityId
                };
            }
        }

        /// <summary>
        /// Creates default event
        /// i.e. Event without a defined price, and being labled as unposted
        /// </summary>
        public async Task<EventDto> CreateDefaultAsync(PurchaseCompleteDto purchaseCompleteDto, Artist artist, Listing listing)
        {
            var artistGenreIds = artist.ArtistGenres.Select(ag => ag.GenreId);
            var listingGenreIds = listing.ListingGenres.Select(lg => lg.GenreId);

            // Genres of the event should be the intersection of the artist, and listing genres
            var matchingGenreIds = listingGenreIds.Any()
                ? artistGenreIds.Intersect(listingGenreIds)
                : artistGenreIds;

            if (!matchingGenreIds.Any())
                throw new BadRequestException("The artist does not match any genres required by the listing");

            var matchingGenres = await genreRepository.GetByIdsAsync(matchingGenreIds);

            var eventEntity = new Event
            {
                ApplicationId = purchaseCompleteDto.EntityId,
                Name = $"{artist.Name} performing at {listing.Venue.Name}",
                About = listing.Venue.About,
                Price = 0,
                TotalTickets = 0,
                AvailableTickets = 0,
                DatePosted = null,
                EventGenres  = matchingGenres
                    .Select(g => new EventGenre
                    {
                        GenreId = g.Id,
                        Genre = g 
                    })
                    .ToList()
            };

            await eventRepository.AddAsync(eventEntity);
            await eventRepository.SaveChangesAsync();

            return mapper.Map<EventDto>(eventEntity);
        }

        public async Task<EventDto> GetDetailsByApplicationIdAsync(int applicationId)
        {
            var eventEntity = await eventRepository.GetByApplicationIdAsync(applicationId);

            if (eventEntity is null)
                throw new NotFoundException($"No event found for Application ID {applicationId}");

            return mapper.Map<EventDto>(eventEntity);
        }

        public async Task<EventDto> UpdateAsync(EventDto eventDto)
        {
            var eventEntity = await eventRepository.GetByIdAsync(eventDto.Id);
            if (eventEntity is null)
                throw new NotFoundException("Event not found");
            
            mapper.Map(eventDto, eventEntity);

            eventRepository.Update(eventEntity);
            await eventRepository.SaveChangesAsync();

            return mapper.Map<EventDto>(eventEntity);
        }

        public async Task<EventPostResponse> PostAsync(EventDto eventDto)
        {
            var eventEntity = await eventRepository.GetByIdAsync(eventDto.Id);

            if (eventEntity is null)
                throw new NotFoundException("Event not found");

            if (eventEntity.DatePosted.HasValue)
                throw new BadRequestException("Event has already been posted");

            mapper.Map(eventDto, eventEntity);

            eventEntity.DatePosted = DateTime.UtcNow;
            eventEntity.AvailableTickets = eventDto.TotalTickets;

            eventRepository.Update(eventEntity);
            await eventRepository.SaveChangesAsync();

            var eventHeaderDto = mapper.Map<EventHeaderDto>(eventDto);

            var averageRating = (await reviewService.GetSummaryByEventIdAsync(eventDto.Id)).AverageRating;

            eventHeaderDto.Rating = averageRating;

            var location = eventEntity.Application.Listing.Venue.Location;

            if (location?.Y == null || location?.X == null)
                return new EventPostResponse
                {
                    Event = eventDto,
                    EventHeader = eventHeaderDto,
                    UserIds = Enumerable.Empty<int>()
                };

            var preferences = await preferenceService.GetAsync();

            var userIdsToNotify = preferences
            .Where(preference =>
             {
                 var inRange = locationService.IsWithinRadius(
                    preference.User.Latitude,
                    preference.User.Longitude,
                    location?.Y,
                    location?.X,
                    preference.RadiusKm);

                 var hasMatchingGenre = preference.Genres.Any(userGenre =>
                     eventDto.Genres.Any(eventGenre => eventGenre.Id == userGenre.Id));

                 return inRange && hasMatchingGenre;
             })
             .Select(preference => preference.User.Id)
             .ToList();


            return new EventPostResponse
            {
                Event = mapper.Map<EventDto>(eventEntity),
                EventHeader = eventHeaderDto,
                UserIds = userIdsToNotify
            };
        }

        public async Task<IEnumerable<EventHeaderDto>> GetRecommendedHeadersAsync()
        {
            var user = await currentUserService.GetOrDefaultAsync();

            if (user is null)
                return Enumerable.Empty<EventHeaderDto>();

            var preferences = await preferenceService.GetByUserIdAsync(user.Id);

            if (preferences is null)
                return Enumerable.Empty<EventHeaderDto>();

            var eventParams = new EventParams
            {
                Latitude = user.Latitude,
                Longitude = user.Longitude,
                RadiusKm = preferences.RadiusKm,
                GenreIds = preferences.Genres.Select(g => g.Id).ToList(),
                OrderByRecent = true,
                Take = 10
            };

            var result = await eventRepository.GetFiltered(user.Id, eventParams);

            await reviewService.AddAverageRatingsAsync(result);

            return result.Take(eventParams.Take);
        }

        public async Task<IEnumerable<EventDto>> GetUnpostedByArtistIdAsync(int id)
        {
            var events = await eventRepository.GetUnpostedByArtistIdAsync(id);
            return mapper.Map<IEnumerable<EventDto>>(events);
        }

        public async Task<IEnumerable<EventDto>> GetUnpostedByVenueIdAsync(int id)
        {
            var events = await eventRepository.GetUnpostedByVenueIdAsync(id);
            return mapper.Map<IEnumerable<EventDto>>(events);
        }
    }
}
