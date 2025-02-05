using Core.Entities;
using Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IArtistService
    {
        Task<IEnumerable<Artist>> GetHeadersAsync(SearchParams searchParams);
        Task<Artist?> GetUserArtist();
    }
}
