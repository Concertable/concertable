using Application.Interfaces;
using Application.Responses;
using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class EventValidationService : IEventValidationService
    {
        private readonly IEventRepository eventRepository;

        public EventValidationService(IEventRepository eventRepository)
        {
            this.eventRepository = eventRepository;
        }

        public async Task<ValidationResponse> CanUpdateAsync(EventDto eventDto)
        {
            var eventEntity = await eventRepository.GetByIdAsync(eventDto.Id);

            if (eventEntity is null)
                return ValidationResponse.Failure("Event not found");

            int ticketsSold = eventEntity.TotalTickets - eventEntity.AvailableTickets;

            if (eventDto.TotalTickets < ticketsSold)
                return ValidationResponse.Failure("Cannot reduce TotalTickets below the number of tickets already sold");

            return ValidationResponse.Success();
        }

        public async Task<ValidationResponse> CanPostAsync(EventDto eventDto)
        {
            if (eventDto.DatePosted is not null)
                return ValidationResponse.Failure("You have already posted this event");

            return ValidationResponse.Success();
        }
    }
}
