using Application.DTOs;
using Application.Requests;
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

namespace Infrastructure.Services;

public class UserPaymentService : IUserPaymentService
{
    private readonly IPaymentService paymentService;
    private IUserService userService;
    private Lazy<IConcertService> concertService;
    private IListingApplicationService listingApplicationService;
    private ICurrentUser currentUser;
    public UserPaymentService(
        IPaymentService paymentService,
        IUserService userService,
        Lazy<IConcertService> concertService,
        IListingApplicationService listingApplicationService,
        ICurrentUser currentUser)
    {
        this.paymentService = paymentService;
        this.userService = userService;
        this.concertService = concertService;
        this.listingApplicationService = listingApplicationService;
        this.currentUser = currentUser;
    }

    public async Task<PaymentResponse> PayVenueManagerByConcertIdAsync(int concertId, int quantity, string paymentMethodId)
    {
        var user = currentUser.Get();
        var toUser = await userService.GetByConcertIdAsync(concertId);
        var concertEntity = await concertService.Value.GetDetailsByIdAsync(concertId);

        var transactionRequestDto = new TransactionRequest
        {
            PaymentMethodId = paymentMethodId,
            FromUserEmail = user.Email,
            Amount = concertEntity.Price * quantity,
            DestinationStripeId = toUser.StripeId,
            Metadata = new Dictionary<string, string>()
        {
            { "fromUserId", user.Id.ToString() },
            { "fromUserEmail", user.Email },
            { "toUserId", toUser.Id.ToString() },
            { "type", "concert" },
            { "concertId", concertId.ToString() },
            { "quantity", quantity.ToString() }
        }
        };

        if (concertEntity == null) throw new NotFoundException("Concert not found");
        if (concertEntity.AvailableTickets <= 0) throw new BadRequestException("No tickets available");

        var paymentResponse = await paymentService.ProcessAsync(transactionRequestDto);

        return paymentResponse;
    }

    public async Task<PaymentResponse> PayArtistManagerByApplicationIdAsync(int applicationId, string paymentMethodId)
    {
        var user = currentUser.Get();
        var toUser = await userService.GetByApplicationIdAsync(applicationId);
        var pay = await listingApplicationService.GetListingPayByIdAsync(applicationId);

        var transactionRequestDto = new TransactionRequest
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
