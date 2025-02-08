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
using Core.Responses;

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

        public async Task<PaginationResponse<VenueHeaderDto>> GetHeadersAsync(SearchParams? searchParams)
        {
            var response = await venueRepository.GetHeadersAsync(searchParams);
            var headersDtos = mapper.Map<IEnumerable<VenueHeaderDto>>(response.Data);
            return new PaginationResponse<VenueHeaderDto>(
                headersDtos,
                response.TotalCount,
                response.PageNumber,
                response.PageSize);
        }

        public async Task<Venue> GetDetailsByIdAsync(int id)
        {
            return await venueRepository.GetByIdAsync(id);
        }

        public async Task<VenueDto> CreateAsync(CreateVenueDto venueDto)
        {
            var venue = mapper.Map<Venue>(venueDto);

            var user = await authService.GetCurrentUser();
            venue.UserId = user.Id;

            var createdVenue = await venueRepository.AddAsync(venue);
            return mapper.Map<VenueDto>(createdVenue);
        }

        public async Task<VenueDto?> GetUserVenueAsync()
        {
            var user = await authService.GetCurrentUser();
            var venue = await venueRepository.GetByUserIdAsync(user.Id);

            return mapper.Map<VenueDto>(venue);
        }
    }
}
