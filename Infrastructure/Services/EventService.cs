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

namespace Infrastructure.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository eventRepository;
        private readonly IAuthService authService;
        private readonly IUserPaymentService userPaymentService;
        private readonly IMessageService messageService;
        private readonly IReviewService reviewService;
        private readonly ILocationService locationService;
        private readonly IListingApplicationService applicationService;
        private readonly IMapper mapper;

        public EventService(
            IEventRepository eventRepository, 
            IAuthService authService,
            IUserPaymentService userPaymentService,
            IMessageService messageService,
            IReviewService reviewService, 
            ILocationService locationService,
            IListingApplicationService applicationService, 
            IMapper mapper)
        {
            this.eventRepository = eventRepository;
            this.authService = authService;
            this.userPaymentService = userPaymentService;
            this.messageService = messageService;
            this.reviewService = reviewService;
            this.locationService = locationService;
            this.applicationService = applicationService;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<EventDto>> GetUpcomingByVenueIdAsync(int id)
        {
            var events = await eventRepository.GetUpcomingByVenueIdAsync(id);
            return mapper.Map<IEnumerable<EventDto>>(events);
        }

        public async Task<PaginationResponse<EventHeaderDto>> GetHeadersAsync(SearchParams searchParams)
        {
            var headers = await eventRepository.GetRawHeadersAsync(searchParams);

            await reviewService.AddAverageRatingsAsync(headers.Data);

            var locationHeaders = locationService.FilterAndSortByNearest(searchParams, headers.Data);

            return new PaginationResponse<EventHeaderDto>(
                locationHeaders,
                headers.TotalCount,
                headers.PageNumber,
                headers.PageSize);
        }

        public async Task<IEnumerable<EventDto>> GetUpcomingByArtistIdAsync(int id)
        {
            var events = await eventRepository.GetUpcomingByArtistIdAsync(id);
            return mapper.Map<IEnumerable<EventDto>>(events);
        }

        public async Task<EventDto> GetDetailsByIdAsync(int id)
        {
            var eventEntity = await eventRepository.GetByIdAsync(id);

            return mapper.Map<EventDto>(eventEntity);
        }

        public async Task<ListingApplicationPurchaseResponse> BookAsync(EventBookingParams bookingParams)
        {
            var user = await authService.GetCurrentUserAsync();
            var role = await authService.GetFirstUserRoleAsync(user);

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
                AvailableTickets = 0,
                Posted = false
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
                ApplicationId = purchaseCompleteDto.EntityId
            };
        }

        public async Task<EventDto> GetDetailsByApplicationIdAsync(int applicationId)
        {
            var eventEntity = await eventRepository.GetByApplicationIdAsync(applicationId);

            if (eventEntity is null)
                throw new NotFoundException($"No event found for Application ID {applicationId}");

            return mapper.Map<EventDto>(eventEntity);
        }
    }
}
