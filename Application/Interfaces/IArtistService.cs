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
    public interface IArtistService
    {
        Task<IEnumerable<ArtistHeaderDto>> GetHeadersAsync(SearchParams searchParams);
        Task<Artist?> GetUserArtist();
    }
}
