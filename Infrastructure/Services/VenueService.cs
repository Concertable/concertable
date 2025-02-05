using Core.Entities;
using Core.Entities.Identity;
using Core.Interfaces;
using Core.Parameters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class VenueService : IVenueService
    {
        private readonly IVenueRepository venueRepository;
        private readonly IAuthService authService;

        public VenueService(IVenueRepository venueRepository, IAuthService authService)
        {
            this.venueRepository = venueRepository;
            this.authService = authService;
        }

        public async Task<IEnumerable<Venue>> GetHeadersAsync(SearchParams? searchParams)
        {
            return await venueRepository.GetHeadersAsync(searchParams);
        }

        public async Task<Venue> GetDetailsByIdAsync(int id)
        {
            return await venueRepository.GetByIdAsync(id);
        }

        public async void Create(Venue venue)
        {
            var user = await authService.GetCurrentUser();
            venue.UserId = user.Id;

            venueRepository.Add(venue);
        }

        public async Task<Venue?> GetUserVenueAsync()
        {
            var user = await authService.GetCurrentUser();
            return await venueRepository.GetByUserIdAsync(user.Id);

        }
    }
}
