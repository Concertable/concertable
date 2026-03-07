using AutoMapper;
using Core.Entities;
using Core.Entities.Identity;
using Application.Interfaces;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Requests;
using Core.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services
{
    public class VenueService : IVenueService
    {
        private readonly IVenueRepository venueRepository;
        private readonly IImageService imageService;
        private readonly IReviewService reviewService;
        private readonly ICurrentUserService currentUserService;
        private readonly IGeocodingService geocodingService;
        private readonly IGeometryProvider geometryService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public VenueService(
            IVenueRepository venueRepository,
            IImageService imageService,
            IReviewService reviewService,
            ICurrentUserService currentUserService,
            IGeocodingService geocodingService,
            IUnitOfWork unitOfWork,
            IGeometryProvider geometryService,
            IMapper mapper)
        {
            this.venueRepository = venueRepository;
            this.imageService = imageService;
            this.reviewService = reviewService;
            this.currentUserService = currentUserService;
            this.geocodingService = geocodingService;
            this.unitOfWork = unitOfWork;
            this.geometryService = geometryService;
            this.mapper = mapper;
        }

        public async Task<VenueDto> GetDetailsByIdAsync(int id)
        {
            var venue = await venueRepository.GetByIdAsync(id);
            var venueDto = mapper.Map<VenueDto>(venue);
            await reviewService.SetAverageRatingAsync(venueDto);
            return venueDto;
        }

        public async Task<VenueDto> CreateAsync(CreateVenueRequest request, IFormFile image)
        {
            var venueRepository = unitOfWork.GetRepository<Venue>();
            var userRepository = unitOfWork.GetBaseRepository<ApplicationUser>();

            var venue = mapper.Map<Venue>(request);
            var user = await currentUserService.GetEntityAsync();

            venue.UserId = user.Id;
            venue.ImageUrl = await imageService.UploadAsync(image);

            await UpdateUserLocationAsync(user, request.Latitude, request.Longitude);

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
                venue.ImageUrl = await imageService.ReplaceAsync(image);

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

        public async Task<int> GetIdForCurrentUserAsync()
        {
            var user = await currentUserService.GetAsync();
            int? id = await venueRepository.GetIdByUserIdAsync(user.Id);

            if (id is null)
                throw new ForbiddenException("You do not own a Venue");

            return id.Value;
        }

        private async Task UpdateUserLocationAsync(ApplicationUser user, double latitude, double longitude)
        {
            var location = await geocodingService.GetLocationAsync(latitude, longitude);
            user.County = location.County;
            user.Town = location.Town;
            user.Location = geometryService.CreatePoint(latitude, longitude);
        }
    }
}
