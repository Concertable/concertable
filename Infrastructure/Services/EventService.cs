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

namespace Infrastructure.Services
{
    public class EventService : HeaderService<Event, EventHeaderDto, IEventRepository>, IEventService
    {
        private readonly IEventRepository eventRepository;
        private readonly ICurrentUserService currentUserService;
        private readonly IUserPaymentService userPaymentService;
        private readonly IMessageService messageService;
        private readonly IReviewService reviewService;
        private readonly ILocationService locationService;
        private readonly IListingApplicationService applicationService;
        private readonly IPreferenceService preferenceService;
        private readonly IMapper mapper;

        public EventService(
            IEventRepository eventRepository, 
            ICurrentUserService currentUserService,
            IUserPaymentService userPaymentService,
            IMessageService messageService,
            IReviewService reviewService, 
            ILocationService locationService,
            IListingApplicationService applicationService, 
            IPreferenceService preferenceService,
            IMapper mapper) : base(eventRepository, locationService)
        {
            this.eventRepository = eventRepository;
            this.currentUserService = currentUserService;
            this.userPaymentService = userPaymentService;
            this.messageService = messageService;
            this.reviewService = reviewService;
            this.locationService = locationService;
            this.applicationService = applicationService;
            this.preferenceService = preferenceService;
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
            var (artistDto, venueDto) = await applicationService.GetArtistAndVenueByIdAsync(purchaseCompleteDto.EntityId);

            var eventEntity = new Event
            {
                ApplicationId = purchaseCompleteDto.EntityId,
                Name = $"{artistDto.Name} performing at {venueDto.Name}",
                Price = 0,
                TotalTickets = 0,
                AvailableTickets = 0
            };

            await eventRepository.AddAsync(eventEntity);
            await eventRepository.SaveChangesAsync();

            var eventDto = mapper.Map<EventDto>(eventEntity);

            await messageService.SendAsync(
                purchaseCompleteDto.FromUserId,
                purchaseCompleteDto.ToUserId,
                "event",
                eventDto.Id,
                "Your Application has been accepted! View your event here"
            );

            return new ListingApplicationPurchaseResponse
            {
                Success = true,
                Message = "Event successfully booked!",
                ApplicationId = purchaseCompleteDto.EntityId,
                Event = eventDto
            };
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

            eventEntity.DatePosted = DateTime.Now;
            eventEntity.AvailableTickets = eventDto.TotalTickets;

            eventRepository.Update(eventEntity);
            await eventRepository.SaveChangesAsync();

            var eventHeaderDto = mapper.Map<EventHeaderDto>(eventDto);

            var averageRating = (await reviewService.GetSummaryByEventIdAsync(eventDto.Id)).AverageRating;

            eventHeaderDto.Rating = averageRating;

            var latitude = eventEntity.Application.Listing.Venue.User.Latitude;
            var longitude = eventEntity.Application.Listing.Venue.User.Longitude;

            if (latitude is null || longitude is null)
                return new EventPostResponse
                {
                    Event = eventDto,
                    EventHeader = eventHeaderDto,
                    UserIds = Enumerable.Empty<int>()
                };

            var preferences = await preferenceService.GetAsync();

            var userIdsToNotify = preferences
            .Where(preference => locationService.IsWithinRadius(
                preference.User.Latitude.Value,
                preference.User.Longitude.Value,
                latitude.Value,
                longitude.Value,
                preference.RadiusKm))
            .Select(preference => preference.User.Id);

            return new EventPostResponse
            {
                Event = eventDto,
                EventHeader = eventHeaderDto,
                UserIds = userIdsToNotify
            };
        }

        public async Task<IEnumerable<EventHeaderDto>> GetLocalHeadersForUserAsync(bool orderByRecent, int? take)
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
                OrderByRecent = orderByRecent,
                Take = take
            };

            var events = await eventRepository.GetFilteredAsync(eventParams);

            var nearbyEvents = locationService.FilterByRadius(user.Latitude.Value, user.Longitude.Value, preferences.RadiusKm, events);

            return take.HasValue ? nearbyEvents.Take(take.Value) : nearbyEvents;
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
