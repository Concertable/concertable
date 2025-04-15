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
using Core.Exceptions;
using Microsoft.AspNetCore.Http;
using NetTopologySuite.Geometries;

namespace Infrastructure.Services
{
    public class VenueService : HeaderService<Venue, VenueHeaderDto, IVenueRepository>, IVenueService
    {
        private readonly IVenueRepository venueRepository;
        private readonly IBlobStorageService blobStorageService;
        private readonly ILocationService locationService;
        private readonly IReviewService reviewService;
        private readonly ICurrentUserService currentUserService;
        private readonly IGeocodingService geocodingService;
        private IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public VenueService(
            IVenueRepository venueRepository,
            IBlobStorageService blobStorageService,
            IReviewService reviewService,
            ICurrentUserService currentUserService,
            IGeocodingService geocodingService,
            IUnitOfWork unitOfWork,
            IGeometryService geometryService,
            IMapper mapper)
            : base(venueRepository, geometryService)
        {
            this.venueRepository = venueRepository;
            this.blobStorageService = blobStorageService;
            this.reviewService = reviewService;
            this.currentUserService = currentUserService;
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

            var venueDto = mapper.Map<VenueDto>(venue);
            await reviewService.SetAverageRatingAsync(venueDto);

            return venueDto;
        }

        public async Task<VenueDto> CreateAsync(CreateVenueDto createVenueDto, IFormFile image)
        {
            var venueRepository = unitOfWork.GetRepository<Venue>();
            var userRepository = unitOfWork.GetBaseRepository<ApplicationUser>();

            var venue = mapper.Map<Venue>(createVenueDto);
            var user = await currentUserService.GetEntityAsync();

            venue.UserId = user.Id;
            venue.ImageUrl = await UploadImageAsync(image);

            await UpdateUserLocationAsync(user, createVenueDto.Latitude, createVenueDto.Longitude);

            var createdVenue = await venueRepository.AddAsync(venue);
            userRepository.Update(user);
            await unitOfWork.SaveChangesAsync();

            return mapper.Map<VenueDto>(createdVenue);
        }

        public async Task<VenueDto> UpdateAsync(VenueDto venueDto, IFormFile? image)
        {
            var venueRepository = unitOfWork.GetRepository<Venue>();
            var userRepository = unitOfWork.GetBaseRepository<ApplicationUser>();

            var averageRating = venueDto.Rating;

            var venue = await venueRepository.GetByIdAsync(venueDto.Id);
            var user = await currentUserService.GetEntityAsync();

            if (venue?.UserId != user.Id)
                throw new ForbiddenException("You do not own this venue");

            mapper.Map(venueDto, venue);

            await UpdateUserLocationAsync(user, venueDto.Latitude, venueDto.Longitude);

            if (image is not null)
            {
                await blobStorageService.DeleteAsync(venue.ImageUrl); // Delete old image
                venue.ImageUrl = await UploadImageAsync(image, venue.ImageUrl); // Replace with new Image
            }

            venueRepository.Update(venue);
            userRepository.Update(user);

            await unitOfWork.SaveChangesAsync();

            mapper.Map(venue, venueDto);
            venueDto.Rating = averageRating;
            return venueDto;
        }

        public async Task<VenueDto?> GetDetailsForCurrentUserAsync()
        {
            var user = await currentUserService.GetAsync();
            var venue = await venueRepository.GetByUserIdAsync(user.Id);

            var venueDto = mapper.Map<VenueDto>(venue);
            await reviewService.SetAverageRatingAsync(venueDto);

            return venueDto;
        }

        private async Task UpdateUserLocationAsync(ApplicationUser user, double latitude, double longitude)
        {
            var location = await geocodingService.GetLocationAsync(latitude, longitude);

            user.County = location.County;
            user.Town = location.Town;
            user.Location = geometryService.CreatePoint(latitude, longitude);
        }

        private async Task<string> UploadImageAsync(IFormFile image, string? oldImageUrl = null)
        {
            if (!string.IsNullOrEmpty(oldImageUrl))
            {
                await blobStorageService.DeleteAsync(oldImageUrl); // Deletes old image (if there is one)
            }

            var extension = Path.GetExtension(image.FileName);
            var imageUrl = $"{Guid.NewGuid()}{extension}"; // Prevents collisions

            using var stream = image.OpenReadStream();
            await blobStorageService.UploadAsync(stream, imageUrl); // Upload new image

            return imageUrl;
        }
    }
}
