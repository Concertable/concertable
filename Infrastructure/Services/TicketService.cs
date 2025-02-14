using Application.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Application.Interfaces;
using Core.Entities;
using Core.Responses;
using System;
using System.Threading.Tasks;
using Application.DTOs;
using Core.Exceptions;
using Core.Parameters;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Infrastructure.Services
{
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
                FromUserId = user.Id,
                FromUserEmail = user.Email,
                ToUserId = toUserId,
                Amount = eventEntity.Price,
                PaymentType = "ticket"
            };

            if (eventEntity == null)
                throw new NotFoundException("Event not found");

            if (eventEntity.AvailableTickets <= 0)
                throw new BadRequestException("No tickets available");

            // Process payment
            var paymentResponse = await paymentService.ProcessAsync(paymentMethodId, transactionDto);
            if (!paymentResponse.Success)
                throw new PaymentRequiredException("Payment failed");

            // Create and save the ticket
            var ticket = new Ticket
            {
                UserId = user.Id,
                EventId = eventId,
                PurchaseDate = DateTime.Now
            };

            await ticketRepository.AddAsync(ticket);

            // Reduce available tickets and save changes
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
                TransactionId = paymentResponse.TransactionId,
                UserEmail = user.Email
            };
        }
    }
}

