using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IReviewService
    {
        Task AddRatingsAsync(IEnumerable<ArtistHeaderDto> headers);
        Task AddRatingsAsync(IEnumerable<VenueHeaderDto> headers);
        Task AddRatingsAsync(IEnumerable<EventHeaderDto> headers);
    }
}
