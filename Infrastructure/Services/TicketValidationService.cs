using Application.Interfaces;
using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class TicketValidationService : ITicketValidationService
    {
        private readonly IEventService eventService;

        public TicketValidationService(IEventService eventService)
        {
            this.eventService = eventService;
        }

        public async Task<ValidationResponse> CanPurchaseTicketAsync(int eventId)
        {
            var eventEntity = await eventService.GetDetailsByIdAsync(eventId);

            if (eventEntity is null)
                return ValidationResponse.Failure("Event does not exist");

            if (eventEntity.StartDate < DateTime.UtcNow)
                return ValidationResponse.Failure("You cannot purchase a Ticket for an Event that's already passed");

            if (eventEntity.DatePosted is null)
                return ValidationResponse.Failure("Event has not yet been posted");

            return ValidationResponse.Success();
        }
    }
}
