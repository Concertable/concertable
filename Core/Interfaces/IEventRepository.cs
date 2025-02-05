using Core.Entities;
using Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IEventRepository : IBaseEntityRepository<Event>
    {
        Task<IEnumerable<Event>> GetUpcomingByVenueIdAsync(int id);
        Task<IEnumerable<Event>> GetHeadersAsync(SearchParams searchParams);
    }
}
