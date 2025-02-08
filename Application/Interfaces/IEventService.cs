using Core.Entities;
using Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IEventService
    {
        Task<IEnumerable<EventDto>> GetUpcomingByVenueIdAsync(int id);
        Task<IEnumerable<EventDto>> GetUpcomingByArtistIdAsync(int id);
        Task<IEnumerable<EventHeaderDto>> GetHeadersAsync(SearchParams searchParams);
    }
}
