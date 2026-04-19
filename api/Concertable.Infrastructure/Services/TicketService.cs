using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Mappers;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Application.Exceptions;
using Concertable.Core.Parameters;
using FluentResults;

public class TicketService : ITicketService
{
    private readonly ITicketRepository ticketRepository;
    private readonly ITicketValidator ticketValidator;
    private readonly IUnitOfWork unitOfWork;
    private readonly ITicketPaymentDispatcher ticketPaymentDispatcher;
    private readonly IEmailService emailService;
    private readonly IQrCodeService qrCodeService;
    private readonly ICurrentUser currentUser;
    private readonly IConcertRepository concertRepository;
    private readonly TimeProvider timeProvider;

    public TicketService(
        ITicketRepository ticketRepository,
        ITicketValidator ticketValidator,
        IUnitOfWork unitOfWork,
        ITicketPaymentDispatcher ticketPaymentDispatcher,
        IEmailService emailService,
        IQrCodeService qrCodeService,
        ICurrentUser currentUser,
        IConcertRepository concertRepository,
        TimeProvider timeProvider)
    {
        this.ticketRepository = ticketRepository;
        this.ticketValidator = ticketValidator;
        this.unitOfWork = unitOfWork;
        this.ticketPaymentDispatcher = ticketPaymentDispatcher;
        this.emailService = emailService;
        this.qrCodeService = qrCodeService;
        this.currentUser = currentUser;
        this.concertRepository = concertRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<Result<TicketPaymentResponse>> PurchaseAsync(TicketPurchaseParams purchaseParams)
    {
        var user = currentUser.Get();

        if (user.Role != Role.Customer)
            throw new ForbiddenException("Only Customers can buy tickets");

        var validationResult = await ticketValidator.CanPurchaseTicketAsync(purchaseParams.ConcertId, purchaseParams.Quantity);

        if (validationResult.IsFailed)
            throw new BadRequestException(validationResult.Errors);

        var concert = await concertRepository.GetByIdAsync(purchaseParams.ConcertId)
            ?? throw new NotFoundException("Concert not found");

        var paymentResult = await ticketPaymentDispatcher.PayAsync(purchaseParams.ConcertId, purchaseParams.Quantity, purchaseParams.PaymentMethodId, concert.Price);

        if (paymentResult.IsFailed)
            return Result.Fail(paymentResult.Errors);

        return Result.Ok(new TicketPaymentResponse
        {
            RequiresAction = paymentResult.Value.RequiresAction,
            TransactionId = paymentResult.Value.TransactionId,
            ClientSecret = paymentResult.Value.ClientSecret,
            UserEmail = user.Email
        });
    }

    public async Task<Result<TicketPaymentResponse>> CompleteAsync(PurchaseCompleteDto purchaseCompleteDto)
    {
        var concertEntity = await concertRepository.GetByIdAsync(purchaseCompleteDto.EntityId);

        if (concertEntity is null)
            return Result.Fail("Concert not found");

        using var transaction = await unitOfWork.BeginTransactionAsync();

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

            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
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
            TransactionId = purchaseCompleteDto.TransactionId,
            UserEmail = purchaseCompleteDto.FromEmail
        });
    }

public async Task<IEnumerable<TicketDto>> GetUserUpcomingAsync()
    {
        var user = currentUser.Get();
        var tickets = await ticketRepository.GetUpcomingByUserIdAsync(user.Id);
        return tickets.ToDtos();
    }

    public async Task<IEnumerable<TicketDto>> GetUserHistoryAsync()
    {
        var user = currentUser.Get();
        var tickets = await ticketRepository.GetHistoryByUserIdAsync(user.Id);
        return tickets.ToDtos();
    }

    private TicketEntity BuildTicket(Guid userId, int concertId)
    {
        var ticketId = Guid.CreateVersion7();
        var qrCode = qrCodeService.GenerateFromTicketId(ticketId);
        return TicketEntity.Create(ticketId, userId, concertId, qrCode, timeProvider.GetUtcNow().DateTime);
    }
}
