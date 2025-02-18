using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<double?> GetRatingByEventIdAsync(int id);
        Task<double?> GetRatingByArtistIdAsync(int id);
        Task<double?> GetRatingByVenueIdAsync(int id);
    }
}
