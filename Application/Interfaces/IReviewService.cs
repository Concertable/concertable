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
        Task AddArtistRatingsAsync(IEnumerable<ArtistHeaderDto> headers);
        Task AddVenueRatingsAsync(IEnumerable<VenueHeaderDto> headers);
        Task AddEventRatingsAsync(IEnumerable<EventHeaderDto> headers);
    }
}
