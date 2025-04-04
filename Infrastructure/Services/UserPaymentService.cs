using Application.DTOs;
using Application.Interfaces;
using Core.Exceptions;
using Application.Responses;
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
        private Lazy<IEventService> eventService;
        private IListingApplicationService listingApplicationService;
        private ICurrentUserService currentUserService;
        private IVenueService venueService;

        public UserPaymentService(
            IPaymentService paymentService, 
            IUserService userService,
            Lazy<IEventService> eventService,
            IListingApplicationService listingApplicationService,
            ICurrentUserService currentUserService)
        {
            this.paymentService = paymentService;
            this.userService = userService;
            this.eventService = eventService;
            this.listingApplicationService = listingApplicationService;
            this.currentUserService = currentUserService;
        }

        public async Task<PaymentResponse> PayVenueManagerByEventIdAsync(int eventId, int quantity, string paymentMethodId) 
        {
            var user = await currentUserService.GetAsync();
            var toUser = await userService.GetByEventIdAsync(eventId);
            var eventEntity = await eventService.Value.GetDetailsByIdAsync(eventId);

            var transactionRequestDto = new TransactionRequestDto
            {
                PaymentMethodId = paymentMethodId,
                FromUserEmail = user.Email,
                Amount = eventEntity.Price,
                DestinationStripeId = toUser.StripeId,
                Metadata = new Dictionary<string, string>()
            {
                { "fromUserId", user.Id.ToString() },
                { "fromUserEmail", user.Email },
                { "toUserId", toUser.Id.ToString() },
                { "type", "event" },
                { "eventId", eventId.ToString() },
                { "quantity", quantity.ToString() }
            }
            };

            if (eventEntity == null) throw new NotFoundException("Event not found");
            if (eventEntity.AvailableTickets <= 0) throw new BadRequestException("No tickets available");

            var paymentResponse = await paymentService.ProcessAsync(transactionRequestDto);

            return paymentResponse;
        }

        public async Task<PaymentResponse> PayArtistManagerByApplicationIdAsync(int applicationId, string paymentMethodId)
        {
            var user = await currentUserService.GetAsync();
            var toUser = await userService.GetByApplicationIdAsync(applicationId);
            var pay = await listingApplicationService.GetListingPayByIdAsync(applicationId);

            var transactionRequestDto = new TransactionRequestDto
            {
                PaymentMethodId = paymentMethodId,
                FromUserEmail = user.Email,
                Amount = pay,
                DestinationStripeId = toUser.StripeId,
                Metadata = new Dictionary<string, string>()
            {
                { "fromUserId", user.Id.ToString() },
                { "fromUserEmail", user.Email },
                { "toUserId", toUser.Id.ToString() },
                { "type", "application" },
                { "applicationId", applicationId.ToString() }
            }
            };

            var paymentResponse = await paymentService.ProcessAsync(transactionRequestDto);
            return paymentResponse;
        }

    }
}
