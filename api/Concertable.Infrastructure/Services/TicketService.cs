using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Concert;
using Application.Mappers;
using Core.Entities;
using Core.Enums;
using Core.Exceptions;
using Application.Responses;
using Core.Parameters;

public class TicketService : ITicketService
{
    private readonly ITicketRepository ticketRepository;
    private readonly ITicketValidator ticketValidator;
    private readonly IUnitOfWork unitOfWork;
    private readonly IContractServiceFactory<ITicketPaymentService> paymentServiceFactory;
    private readonly IEmailService emailService;
    private readonly IQrCodeService qrCodeService;
    private readonly ICurrentUser currentUser;
    private readonly IConcertRepository concertRepository;
    private readonly TimeProvider timeProvider;

    public TicketService(
        ITicketRepository ticketRepository,
        ITicketValidator ticketValidator,
        IUnitOfWork unitOfWork,
        IContractServiceFactory<ITicketPaymentService> paymentServiceFactory,
        IEmailService emailService,
        IQrCodeService qrCodeService,
        ICurrentUser currentUser,
        IConcertRepository concertRepository,
        TimeProvider timeProvider)
    {
        this.ticketRepository = ticketRepository;
        this.ticketValidator = ticketValidator;
        this.unitOfWork = unitOfWork;
        this.paymentServiceFactory = paymentServiceFactory;
        this.emailService = emailService;
        this.qrCodeService = qrCodeService;
        this.currentUser = currentUser;
        this.concertRepository = concertRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<TicketPurchaseResponse> PurchaseAsync(TicketPurchaseParams purchaseParams)
    {
        var user = currentUser.Get();

        if (user.Role != Role.Customer)
            throw new ForbiddenException("Only Customers can buy tickets");

        var result = await ticketValidator.CanPurchaseTicketAsync(purchaseParams.ConcertId, purchaseParams.Quantity);

        if (!result.IsValid)
            throw new BadRequestException(result.Errors);

        var concert = await concertRepository.GetByIdAsync(purchaseParams.ConcertId)
            ?? throw new NotFoundException("Concert not found");
        var contractType = await concertRepository.GetTypeByIdAsync(purchaseParams.ConcertId)
            ?? throw new NotFoundException("Concert contract not found");
        var paymentService = paymentServiceFactory.Create(contractType);
        var paymentResponse = await paymentService.PayAsync(purchaseParams.ConcertId, purchaseParams.Quantity, purchaseParams.PaymentMethodId, concert.Price);

        return new TicketPurchaseResponse
        {
            Success = paymentResponse.Success,
            RequiresAction = paymentResponse.RequiresAction,
            Message = paymentResponse.Message ?? (paymentResponse.Success ? "Payment successful" : "Payment failed"),
            TransactionId = paymentResponse.TransactionId,
            UserEmail = user.Email,
            ClientSecret = paymentResponse.ClientSecret
        };
    }

    public async Task<TicketPurchaseResponse> CompleteAsync(PurchaseCompleteDto purchaseCompleteDto)
    {
        var concertEntity = await concertRepository.GetByIdAsync(purchaseCompleteDto.EntityId)
            ?? throw new NotFoundException("Concert not found");

        using var transaction = await unitOfWork.BeginTransactionAsync();

        int quantity = purchaseCompleteDto.Quantity ?? 1;
        var tickets = new List<TicketEntity>();

        try
        {
            for (int i = 0; i < quantity; i++)
            {
                var ticket = new TicketEntity
                {
                    UserId = purchaseCompleteDto.FromUserId,
                    ConcertId = purchaseCompleteDto.EntityId,
                    PurchaseDate = timeProvider.GetUtcNow().DateTime
                };

                var ticketResponse = await ticketRepository.AddAsync(ticket);
                await unitOfWork.SaveChangesAsync();

                ticket.QrCode = qrCodeService.GenerateFromTicketId(ticketResponse.Id);
                ticketRepository.Update(ticket);

                tickets.Add(ticket);
            }

            concertEntity.AvailableTickets -= quantity;
            concertRepository.Update(concertEntity);

            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return new TicketPurchaseResponse
            {
                Message = "Failed to Create Ticket. Please contact support",
                ConcertId = purchaseCompleteDto.EntityId,
            };
        }

        if (!tickets.Any())
            throw new NotFoundException("No Tickets found");

        var ticketIds = tickets.Select(t => t.Id);
        await emailService.SendTicketsToEmailAsync(purchaseCompleteDto.FromEmail, ticketIds);

        return new TicketPurchaseResponse
        {
            Success = true,
            Message = "Ticket purchased successfully!",
            TicketIds = ticketIds,
            ConcertId = purchaseCompleteDto.EntityId,
            PurchaseDate = tickets[0].PurchaseDate,
            Amount = concertEntity.Price,
            Currency = "GBP",
            TransactionId = purchaseCompleteDto.TransactionId,
            UserEmail = purchaseCompleteDto.FromEmail
        };
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
}
