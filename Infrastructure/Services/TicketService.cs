using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Core.Exceptions;
using Application.Responses;
using Infrastructure.Services;
using Core.Parameters;
using AutoMapper;

public class TicketService : ITicketService
{
    private readonly ITicketRepository ticketRepository;
    private readonly ITicketValidationService ticketValidationService;
    private readonly IUnitOfWork unitOfWork;
    private readonly IUserPaymentService userPaymentService;
    private readonly IEmailService emailService;
    private readonly IQrCodeService qrCodeService;
    private readonly ICurrentUserService currentUserService;
    private readonly IManagerService managerService;
    private readonly IMapper mapper;

    public TicketService(
        ITicketRepository ticketRepository,
        ITicketValidationService ticketValidationService,
        IUnitOfWork unitOfWork,
        IUserPaymentService userPaymentService,
        IEmailService emailService,
        IQrCodeService qrCodeService,
        ICurrentUserService currentUserService,
        IManagerService managerService,
        IMapper mapper
        )
    {
        this.ticketRepository = ticketRepository;
        this.ticketValidationService = ticketValidationService;
        this.unitOfWork = unitOfWork;
        this.userPaymentService = userPaymentService;
        this.emailService = emailService;
        this.qrCodeService = qrCodeService;
        this.currentUserService = currentUserService;
        this.managerService = managerService;
        this.mapper = mapper;
    }

    public async Task<TicketPurchaseResponse> PurchaseAsync(TicketPurchaseParams purchaseParams)
    {
        var user = await currentUserService.GetAsync();
        var role = await currentUserService.GetFirstRoleAsync();

        if (role != "Customer")
            throw new ForbiddenException("Only Customers can buy tickets");

        var response =  await ticketValidationService.CanPurchaseTicketAsync(purchaseParams.EventId);

        if (!response.IsValid)
            throw new BadRequestException(response.Reason!);

        var paymentResponse = await userPaymentService.PayVenueManagerByEventIdAsync(purchaseParams.EventId, purchaseParams.Quantity , purchaseParams.PaymentMethodId);

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

    /// <summary>
    /// Called by the Webhook controller once the payment process is complete
    /// Adds the Ticket to the Database whilst updating Event Data
    /// </summary>
    public async Task<TicketPurchaseResponse> CompleteAsync(PurchaseCompleteDto purchaseCompleteDto)
    {
        var ticketRepository = unitOfWork.GetRepository<Ticket>();
        var eventRepository = unitOfWork.GetRepository<Event>();

        var eventEntity = await eventRepository.GetByIdAsync(purchaseCompleteDto.EntityId);

        using var transaction = await unitOfWork.BeginTransactionAsync();

        int quantity = purchaseCompleteDto.Quantity ?? 1;

        var tickets = new List<Ticket>();

        try
        {
            for (int i = 0; i < quantity; i++)
            {
                var ticket = new Ticket
                {
                    UserId = purchaseCompleteDto.FromUserId,
                    EventId = purchaseCompleteDto.EntityId,
                    PurchaseDate = DateTime.Now
                };

                var ticketResponse = await ticketRepository.AddAsync(ticket);
                await unitOfWork.SaveChangesAsync(); // Required to get ticketResponse.Id

                // Adds newly created QR Code to the ticket
                ticket.QrCode = qrCodeService.GenerateFromTicketId(ticketResponse.Id);
                ticketRepository.Update(ticket);

                tickets.Add(ticket);
            }

            // Reduce available tickets and save changes
            // Made Sure tickets are available in PurchaseAsync method
            eventEntity.AvailableTickets -= quantity;
            eventRepository.Update(eventEntity);

            // Save & Commit Changes if all db operations executed without error
            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            /* 
             * Since we are forced to save midway through unitOfWork,
             * Rollback if the any of the try block fails
             */
            await transaction.RollbackAsync();
            return new TicketPurchaseResponse
            {
                Message = "Failed to Create Ticket. Please contact support",
                EventId = purchaseCompleteDto.EntityId,
            };
        }

        if(!tickets.Any())
            throw new NotFoundException("No Tickets found");

        var ticketIds = tickets.Select(t => t.Id);
        await emailService.SendTicketsToEmailAsync(purchaseCompleteDto.FromUserId, purchaseCompleteDto.FromEmail, ticketIds);

        return new TicketPurchaseResponse
        {
            Success = true,
            Message = "Ticket purchased successfully!",
            TicketIds = ticketIds,
            EventId = purchaseCompleteDto.EntityId,
            PurchaseDate = tickets[0].PurchaseDate,
            Amount = eventEntity.Price,
            Currency = "GBP",
            TransactionId = purchaseCompleteDto.TransactionId,
            UserEmail = purchaseCompleteDto.FromEmail
        };
    }

    public Task<byte[]> GetQrCodeByIdAsync(int id)
    {
        return ticketRepository.GetQrCodeByIdAsync(id);
    }

    public async Task<IEnumerable<TicketDto>> GetUserUpcomingAsync()
    {
        var user = await currentUserService.GetAsync();

        var tickets = await ticketRepository.GetUpcomingByUserIdAsync(user.Id);

        return mapper.Map<IEnumerable<TicketDto>>(tickets);
    }

    public async Task<IEnumerable<TicketDto>> GetUserHistoryAsync()
    {
        var user = await currentUserService.GetAsync();

        var tickets = await ticketRepository.GetHistoryByUserIdAsync(user.Id);

        return mapper.Map<IEnumerable<TicketDto>>(tickets);
    }
}
