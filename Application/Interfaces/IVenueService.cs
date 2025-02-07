using Core.Entities;
using Core.Entities.Identity;
using Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IVenueService
    {
        Task<IEnumerable<VenueHeaderDto>> GetHeadersAsync(SearchParams? searchParams);

        Task<Venue> GetDetailsByIdAsync(int id);

        Task<Venue?> GetUserVenueAsync();

        void Create(Venue venue);
    }
}
