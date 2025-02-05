using Core.Entities;
using Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IHeaderService
    {
        Task<IEnumerable<Venue>> GetVenueHeadersAsync(SearchParams searchParams);
        Task<IEnumerable<Artist>> GetArtistHeadersAsync(SearchParams searchParams);
        Task<IEnumerable<Event>> GetEventHeadersAsync(SearchParams searchParams);
    }
}
