using AutoMapper;
using Core.Entities;
using Core.Entities.Identity;
using Application.Interfaces;
using Core.Parameters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;

namespace Infrastructure.Services
{
    public class VenueService : IVenueService
    {
        private readonly IVenueRepository venueRepository;
        private readonly IAuthService authService;
        private readonly IMapper mapper;

        public VenueService(IVenueRepository venueRepository, IAuthService authService, IMapper mapper)
        {
            this.venueRepository = venueRepository;
            this.authService = authService;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<VenueHeaderDto>> GetHeadersAsync(SearchParams? searchParams)
        {
            var headers = await venueRepository.GetHeadersAsync(searchParams);
            return mapper.Map<IEnumerable<VenueHeaderDto>>(headers);
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
