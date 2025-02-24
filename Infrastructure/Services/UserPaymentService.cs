using Application.DTOs;
using Application.Interfaces;
using Core.Exceptions;
using Core.Responses;
using Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class UserPaymentService : IUserPaymentService
    {
        private readonly IPaymentService paymentService;
        private IUserService userService;
        private IEventService eventService;
        private IListingApplicationService listingApplicationService;
        private IAuthService authService;
        private IVenueService venueService;

        public UserPaymentService(
            IPaymentService paymentService, 
            IUserService userService,
            IEventService eventService,
            IListingApplicationService listingApplicationService,
            IAuthService authService)
        {
            this.paymentService = paymentService;
            this.userService = userService;
            this.eventService = eventService;
            this.listingApplicationService = listingApplicationService;
            this.authService = authService;
        }

        public async Task<PaymentResponse> PayVenueManagerByEventIdAsync(int eventId, string paymentMethodId) 
        {
            var user = await authService.GetCurrentUserAsync();
            var toUserId = await userService.GetIdByEventIdAsync(eventId);
            var eventEntity = await eventService.GetDetailsByIdAsync(eventId);

            var transactionRequestDto = new TransactionRequestDto
            {
                PaymentMethodId = paymentMethodId,
                FromUserEmail = user.Email,
                Amount = eventEntity.Price,
                Metadata = new Dictionary<string, string>()
            {
                { "fromUserId", user.Id.ToString() },
                { "fromUserEmail", user.Email },
                { "toUserId", toUserId.ToString() },
                { "type", "event" },
                { "eventId", eventId.ToString() }
            }
            };

            if (eventEntity == null) throw new NotFoundException("Event not found");
            if (eventEntity.AvailableTickets <= 0) throw new BadRequestException("No tickets available");

            var paymentResponse = await paymentService.ProcessAsync(transactionRequestDto);

            return paymentResponse;
        }

        public async Task<PaymentResponse> PayArtistManagerByApplicationIdAsync(int applicationId, string paymentMethodId)
        {
            var user = await authService.GetCurrentUserAsync();
            var toUserId = await userService.GetIdByApplicationIdAsync(applicationId);
            var pay = await listingApplicationService.GetListingPayByIdAsync(applicationId);

            var transactionRequestDto = new TransactionRequestDto
            {
                PaymentMethodId = paymentMethodId,
                FromUserEmail = user.Email,
                Amount = pay,
                Metadata = new Dictionary<string, string>()
            {
                { "fromUserId", user.Id.ToString() },
                { "fromUserEmail", user.Email },
                { "toUserId", toUserId.ToString() },
                { "type", "application" },
                { "applicationId", applicationId.ToString() }
            }
            };

            var paymentResponse = await paymentService.ProcessAsync(transactionRequestDto);
            return paymentResponse;
        }

    }
}
