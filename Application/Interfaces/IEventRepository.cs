using Application.DTOs;
using Core.Entities;
using Core.Parameters;
using Application.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IEventRepository : IHeaderRepository<Event, EventHeaderDto>
    {
        Task<Event> GetByIdAsync(int id);
        Task<Event> GetByApplicationIdAsync(int id);
        Task<IEnumerable<Event>> GetUpcomingByVenueIdAsync(int id);
        Task<IEnumerable<Event>> GetUpcomingByArtistIdAsync(int id);
    }
}
