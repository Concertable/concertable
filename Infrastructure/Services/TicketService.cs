using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Core.Exceptions;
using Core.Responses;

public class TicketService : ITicketService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IPaymentService paymentService;
    private readonly IAuthService authService;
    private readonly IManagerService managerService;

    public TicketService(
        IUnitOfWork unitOfWork,
        IPaymentService paymentService,
        IAuthService authService,
        IManagerService managerService
        )
    {
        this.unitOfWork = unitOfWork;
        this.paymentService = paymentService;
        this.authService = authService;
        this.managerService = managerService;
    }

    public async Task<TicketPurchaseResponse> PurchaseAsync(string paymentMethodId, int eventId)
    {
        var ticketRepository = unitOfWork.GetRepository<Ticket>();
        var eventRepository = unitOfWork.GetRepository<Event>();

        var user = await authService.GetCurrentUserAsync();
        var eventEntity = await eventRepository.GetByIdAsync(eventId);
        var toUserId = await managerService.GetIdByEventIdAsync(eventId);

        var transactionDto = new TransactionDto
        {
            FromUserEmail = user.Email,
            Amount = eventEntity.Price,
            Metadata =
            {
                { "fromUserId", user.Id.ToString() },
                { "toUserId", toUserId.ToString() },
                { "type", "event" },
                { "eventId", eventId.ToString() }
            }
        };

        if (eventEntity == null) throw new NotFoundException("Event not found");
        if (eventEntity.AvailableTickets <= 0) throw new BadRequestException("No tickets available");

        var paymentResponse = await paymentService.ProcessAsync(transactionDto, paymentMethodId);

        return new TicketPurchaseResponse
        {
            Success = paymentResponse.Success,
            RequiresAction = paymentResponse.RequiresAction,
            Message = paymentResponse.Message ?? (paymentResponse.Success ? "Payment successful" : "Payment failed"),
            TransactionId = paymentResponse.TransactionId,
            UserEmail = user.Email,
            ClientSecret = paymentResponse.ClientSecret // Ensure this is always sent to the frontend
        };
    }



    public async Task<TicketPurchaseResponse> CompleteAsync(string transactionId, int eventId)
    {
        var ticketRepository = unitOfWork.GetRepository<Ticket>();
        var eventRepository = unitOfWork.GetRepository<Event>();

        var user = await authService.GetCurrentUserAsync();
        var eventEntity = await eventRepository.GetByIdAsync(eventId);

        // Create and save the ticket
        var ticket = new Ticket
        {
            UserId = user.Id,
            EventId = eventId,
            PurchaseDate = DateTime.Now
        };

        await ticketRepository.AddAsync(ticket);

        // Reduce available tickets and save changes
        // Made Sure tickets are available in PurchaseAsync method
        eventEntity.AvailableTickets -= 1;
        eventRepository.Update(eventEntity);

        await unitOfWork.SaveChangesAsync();

        return new TicketPurchaseResponse
        {
            Success = true,
            Message = "Ticket purchased successfully!",
            TicketId = ticket.Id,
            EventId = eventId,
            PurchaseDate = ticket.PurchaseDate,
            Amount = eventEntity.Price,
            Currency = "GBP",
            TransactionId = transactionId,
            UserEmail = user.Email
        };
    }
}
