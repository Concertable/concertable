using Application.Interfaces;
using Application.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class TicketValidationService : ITicketValidationService
    {
        private readonly IConcertService concertService;
        private readonly TimeProvider timeProvider;

        public TicketValidationService(IConcertService concertService, TimeProvider timeProvider)
        {
            this.concertService = concertService;
            this.timeProvider = timeProvider;
        }

        public async Task<ValidationResponse> CanPurchaseTicketAsync(int concertId, int? quantity = null)
        {
            var reasons = new List<string>();
            var concertEntity = await concertService.GetDetailsByIdAsync(concertId);

            if (concertEntity is null)
            {
                reasons.Add("Concert does not exist.");
            }
            else
            {
                if(concertEntity.DatePosted is null)
                    reasons.Add("Concert is not posted yet");

                if (concertEntity.StartDate < timeProvider.GetUtcNow())
                    reasons.Add("You cannot purchase a Ticket for a Concert that's already passed");

                if (concertEntity.DatePosted is null)
                    reasons.Add("Concert has not yet been posted");

                if (concertEntity.AvailableTickets <= 0)
                    reasons.Add("No Tickets Available for Concert");

                if (quantity.HasValue && concertEntity.AvailableTickets - quantity.Value < 0)
                    reasons.Add($"Not enough tickets available. Only {concertEntity.AvailableTickets} tickets are available");
            }

            if (reasons.Any())
                return ValidationResponse.Failure(reasons);

            return ValidationResponse.Success();
        }
    }
}
