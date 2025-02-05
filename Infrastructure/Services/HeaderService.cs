using Core.Entities;
using Core.Interfaces;
using Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class HeaderService : IHeaderService
    {
        private readonly IVenueService venueService;
        private readonly IArtistService artistService;
        private readonly IEventService eventService;

        public HeaderService(IVenueService venueService, IArtistService artistService, IEventService eventService)
        {
            this.venueService = venueService;
            this.artistService = artistService;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<Venue>> GetVenueHeadersAsync(SearchParams searchParams)
        {
            return await venueService.GetHeadersAsync(searchParams);
        }

        public async Task<IEnumerable<Artist>> GetArtistHeadersAsync(SearchParams searchParams)
        {
            return await artistService.GetHeadersAsync(searchParams);
        }

        public async Task<IEnumerable<Event>> GetEventHeadersAsync(SearchParams searchParams)
        {
            return await eventService.GetHeadersAsync(searchParams);
        }
    }
}
