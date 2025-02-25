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
using Application.Responses;
using System.Runtime.InteropServices;

namespace Infrastructure.Services
{
    public class VenueService : HeaderService<Venue, VenueHeaderDto, IVenueRepository>, IVenueService
    {
        private readonly IVenueRepository venueRepository;
        private readonly ILocationService locationService;
        private readonly IReviewService reviewService;
        private readonly IAuthService authService;
        private readonly IGeocodingService geocodingService;
        private IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public VenueService(
            IVenueRepository venueRepository,
            ILocationService locationService,
            IReviewService reviewService,
            IAuthService authService,
            IGeocodingService geocodingService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
            : base(venueRepository, locationService)
        {
            this.venueRepository = venueRepository;
            this.locationService = locationService;
            this.reviewService = reviewService;
            this.authService = authService;
            this.geocodingService = geocodingService;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async override Task<PaginationResponse<VenueHeaderDto>> GetHeadersAsync(SearchParams? searchParams)
        {
            var headers = await base.GetHeadersAsync(searchParams);

            await reviewService.AddAverageRatingsAsync(headers.Data);

            return headers;
        }

        public async Task<VenueDto> GetDetailsByIdAsync(int id)
        {
            var venue = await venueRepository.GetByIdAsync(id);
            return mapper.Map<VenueDto>(venue); 
        }

        public async Task<VenueDto> CreateAsync(CreateVenueDto createVenueDto)
        {
            var venueRepository = unitOfWork.GetRepository<Venue>();
            var userRepository = unitOfWork.GetBaseRepository<ApplicationUser>();

            var venue = mapper.Map<Venue>(createVenueDto);

            var user = await authService.GetCurrentUserAsync();
            venue.UserId = user.Id;

            var location = await geocodingService.GetLocationAsync(createVenueDto.Latitude, createVenueDto.Longitude);

            user.County = location.County;
            user.Town = location.Town;

            user.Latitude = createVenueDto.Latitude;
            user.Longitude = createVenueDto.Longitude;

            var createdVenue = await venueRepository.AddAsync(venue);
            userRepository.Update(user);

            await unitOfWork.SaveChangesAsync();

            return mapper.Map<VenueDto>(createdVenue);
        }

        public async Task<VenueDto> UpdateAsync(VenueDto venueDto)
        {
            var venueRepository = unitOfWork.GetRepository<Venue>();
            var userRepository = unitOfWork.GetBaseRepository<ApplicationUser>();

            var venue = mapper.Map<Venue>(venueDto);
            var user = await authService.GetCurrentUserAsync();
            venue.UserId = user.Id;

            var location = await geocodingService.GetLocationAsync(venueDto.Latitude, venueDto.Longitude);

            user.County = location.County;
            user.Town = location.Town;

            venueRepository.Update(venue);
            userRepository.Update(user);

            await unitOfWork.SaveChangesAsync();

            return mapper.Map<VenueDto>(venue);
        }

        public async Task<VenueDto?> GetDetailsForCurrentUserAsync()
        {
            var user = await authService.GetCurrentUserAsync();
            var venue = await venueRepository.GetByUserIdAsync(user.Id);

            return mapper.Map<VenueDto>(venue);
        }
    }
}
