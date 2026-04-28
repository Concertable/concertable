using Concertable.Concert.Application.Interfaces;
using Concertable.Concert.Domain;
using Concertable.Contract.Contracts;
using Concertable.User.Contracts;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using FluentResults;

namespace Concertable.Concert.Infrastructure.Services;

internal class TicketService : ITicketService
{
    private readonly ITicketRepository ticketRepository;
    private readonly ITicketValidator ticketValidator;
    private readonly IEmailService emailService;
    private readonly IQrCodeService qrCodeService;
    private readonly ICurrentUser currentUser;
    private readonly IConcertRepository concertRepository;
    private readonly IContractLookup contractLookup;
    private readonly ITicketPayee ticketPayee;
    private readonly ICustomerPaymentModule customerPaymentModule;
    private readonly TimeProvider timeProvider;

    public TicketService(
        ITicketRepository ticketRepository,
        ITicketValidator ticketValidator,
        IEmailService emailService,
        IQrCodeService qrCodeService,
        ICurrentUser currentUser,
        IConcertRepository concertRepository,
        IContractLookup contractLookup,
        ITicketPayee ticketPayee,
        ICustomerPaymentModule customerPaymentModule,
        TimeProvider timeProvider)
    {
        this.ticketRepository = ticketRepository;
        this.ticketValidator = ticketValidator;
        this.emailService = emailService;
        this.qrCodeService = qrCodeService;
        this.currentUser = currentUser;
        this.concertRepository = concertRepository;
        this.contractLookup = contractLookup;
        this.ticketPayee = ticketPayee;
        this.customerPaymentModule = customerPaymentModule;
        this.timeProvider = timeProvider;
    }

    public async Task<Result<TicketPaymentResponse>> PurchaseAsync(TicketPurchaseParams purchaseParams)
    {
        if (currentUser.GetRole() != Role.Customer)
            throw new ForbiddenException("Only Customers can buy tickets");

        var validationResult = await ticketValidator.CanPurchaseTicketAsync(purchaseParams.ConcertId, purchaseParams.Quantity);
        if (validationResult.IsFailed)
            throw new BadRequestException(validationResult.Errors);

        var concert = await concertRepository.GetFullByIdAsync(purchaseParams.ConcertId)
            ?? throw new NotFoundException("Concert not found");

        var contract = await contractLookup.GetByConcertIdAsync(purchaseParams.ConcertId);
        var payeeUserId = ticketPayee.Resolve(concert, contract);

        var metadata = new Dictionary<string, string>
        {
            ["type"] = "ticket",
            ["concertId"] = purchaseParams.ConcertId.ToString(),
            ["quantity"] = purchaseParams.Quantity.ToString()
        };

        var paymentResult = await customerPaymentModule.PayAsync(
            currentUser.GetId(), payeeUserId,
            concert.Price * purchaseParams.Quantity,
            metadata,
            purchaseParams.PaymentMethodId);

        if (paymentResult.IsFailed)
            return Result.Fail(paymentResult.Errors);

        return Result.Ok(new TicketPaymentResponse
        {
            RequiresAction = paymentResult.Value.RequiresAction,
            TransactionId = paymentResult.Value.TransactionId,
            ClientSecret = paymentResult.Value.ClientSecret,
            UserEmail = currentUser.Email
        });
    }

    public async Task<Result<TicketPaymentResponse>> CompleteAsync(PurchaseCompleteDto purchaseCompleteDto)
    {
        var concertEntity = await concertRepository.GetByIdAsync(purchaseCompleteDto.EntityId);

        if (concertEntity is null)
            return Result.Fail("Concert not found");

        int quantity = purchaseCompleteDto.Quantity ?? 1;
        var tickets = new List<TicketEntity>();

        try
        {
            for (int i = 0; i < quantity; i++)
            {
                var ticket = BuildTicket(purchaseCompleteDto.FromUserId, purchaseCompleteDto.EntityId);
                await ticketRepository.AddAsync(ticket);
                tickets.Add(ticket);
            }

            concertEntity.SellTickets(quantity);
            concertRepository.Update(concertEntity);

            await ticketRepository.SaveChangesAsync();
        }
        catch (Exception)
        {
            return Result.Fail("Failed to create ticket. Please contact support.");
        }

        var ticketIds = tickets.Select(t => t.Id);
        await emailService.SendTicketsToEmailAsync(purchaseCompleteDto.FromEmail, ticketIds);

        return Result.Ok(new TicketPaymentResponse
        {
            TicketIds = ticketIds,
            ConcertId = purchaseCompleteDto.EntityId,
            PurchaseDate = tickets[0].PurchaseDate,
            Amount = concertEntity.Price,
            Currency = "GBP",
            UserEmail = purchaseCompleteDto.FromEmail
        });
    }

    public async Task<Result<TicketCheckout>> CheckoutAsync(int concertId)
    {
        if (currentUser.GetRole() != Role.Customer)
            throw new ForbiddenException("Only Customers can buy tickets");

        var validationResult = await ticketValidator.CanPurchaseTicketAsync(concertId);
        if (validationResult.IsFailed)
            return Result.Fail(validationResult.Errors);

        var concert = await concertRepository.GetFullByIdAsync(concertId)
            ?? throw new NotFoundException("Concert not found");

        var contract = await contractLookup.GetByConcertIdAsync(concertId);
        var payeeUserId = ticketPayee.Resolve(concert, contract);

        var metadata = new Dictionary<string, string>
        {
            ["type"] = "ticket",
            ["concertId"] = concertId.ToString(),
            ["toUserId"] = payeeUserId.ToString(),
            ["amount"] = ((long)(concert.Price * 100)).ToString(),
            ["currency"] = "gbp"
        };

        var session = await customerPaymentModule.CreatePaymentSessionAsync(currentUser.GetId(), metadata);

        return Result.Ok(new TicketCheckout(session, concert.Price, concertId));
    }

    public async Task<IEnumerable<TicketDto>> GetUserUpcomingAsync()
    {
        var tickets = await ticketRepository.GetUpcomingByUserIdAsync(currentUser.GetId());
        return tickets.ToDtos(currentUser.Email ?? string.Empty);
    }

    public async Task<IEnumerable<TicketDto>> GetUserHistoryAsync()
    {
        var tickets = await ticketRepository.GetHistoryByUserIdAsync(currentUser.GetId());
        return tickets.ToDtos(currentUser.Email ?? string.Empty);
    }

    private TicketEntity BuildTicket(Guid userId, int concertId)
    {
        var ticketId = Guid.CreateVersion7();
        var qrCode = qrCodeService.GenerateFromTicketId(ticketId);
        return TicketEntity.Create(ticketId, userId, concertId, qrCode, timeProvider.GetUtcNow().DateTime);
    }
}
