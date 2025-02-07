using Core.Entities;
using Core.Parameters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IEventRepository : IRepository<Event>
    {
        Task<IEnumerable<Event>> GetUpcomingByVenueIdAsync(int id);
        Task<IEnumerable<Event>> GetHeadersAsync(SearchParams searchParams);
    }
}
