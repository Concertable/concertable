using Application.Interfaces;
using Application.Responses;
using System;
using System.Collections.Generic;
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

        public async Task<ValidationResponse> CanPurchaseTicketAsync(int eventId, int? quantity = null)
        {
            var reasons = new List<string>();
            var eventEntity = await eventService.GetDetailsByIdAsync(eventId);

            if (eventEntity is null)
            {
                reasons.Add("Event does not exist.");
            }
            else
            {
                if (eventEntity.StartDate < DateTime.UtcNow)
                    reasons.Add("You cannot purchase a Ticket for an Event that's already passed.");

                if (eventEntity.DatePosted is null)
                    reasons.Add("Event has not yet been posted.");

                if (eventEntity.AvailableTickets <= 0)
                    reasons.Add("No Tickets Available for Event");

                if (quantity.HasValue && eventEntity.AvailableTickets - quantity.Value < 0)
                    reasons.Add($"Not enough tickets available. Only {eventEntity.AvailableTickets} tickets are available.");
            }

            if (reasons.Any())
                return ValidationResponse.Failure(reasons);

            return ValidationResponse.Success();
        }
    }
}
