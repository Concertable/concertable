using Core.Entities;
using Core.Interfaces;
using Core.Parameters;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            this.eventRepository = eventRepository;
        }

        public async Task<IEnumerable<Event>> GetUpcomingByVenueIdAsync(int id)
        {
            return await eventRepository.GetUpcomingByVenueIdAsync(id);
        }

        public async Task<IEnumerable<Event>> GetHeadersAsync(SearchParams searchParams)
        {
            return await eventRepository.GetHeadersAsync(searchParams);
        }
    }
}
