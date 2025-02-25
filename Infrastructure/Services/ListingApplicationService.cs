using Core.Entities;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using AutoMapper;
using Core.Exceptions;
using Core.Enums;

namespace Infrastructure.Services
{
    public class ListingApplicationService : IListingApplicationService
    {
        private readonly IListingApplicationRepository listingApplicationRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAuthService authService;
        private readonly IMessageService messageService;
        private readonly IListingService listingService;
        private readonly IArtistService artistService;
        private readonly IMapper mapper;

        public ListingApplicationService(
            IListingApplicationRepository listingApplicationRepository,
            IUnitOfWork unitOfWork,
            IAuthService authService,
            IMessageService messageService,
            IListingService listingService,
            IArtistService artistService,
            IMapper mapper)
        {
            this.listingApplicationRepository = listingApplicationRepository;
            this.unitOfWork = unitOfWork;
            this.authService = authService;
            this.messageService = messageService;
            this.listingService = listingService;
            this.artistService = artistService;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<ListingApplicationDto>> GetAllForListingIdAsync(int id)
        {
            var applications = await listingApplicationRepository.GetAllForListingIdAsync(id);

            return mapper.Map<IEnumerable<ListingApplicationDto>>(applications);
        }

        public async Task ApplyForListingAsync(int listingId)
        {
            // Prepare data for Service calls
            var artistDto = await artistService.GetDetailsForCurrentUserAsync();

            if (artistDto is null)
                throw new ForbiddenException("You must create an Artist account before you apply for a listing");

            var application = new ListingApplication()
            {
                ListingId = listingId,
                ArtistId = artistDto.Id,
            };
            var user = await authService.GetCurrentUserAsync();
            var listingOwner = await listingService.GetOwnerByIdAsync(listingId);

            var applicationRepository = unitOfWork.GetRepository<ListingApplication>();

            // Add to application table
            await applicationRepository.AddAsync(application);

            // Send message to venue owner
            await messageService.SendAsync(
                fromUserId: user.Id, 
                toUserId: listingOwner.Id,
                content: $"{user.Email} has applied to your listing",
                action: "application",
                actionId: listingId); //?

            // Save changes after both have executed
            try
            {
                await unitOfWork.TrySaveChangesAsync();
            }
            catch(BadRequestException ex) when (ex.ErrorType == ErrorType.DuplicateKey)
            {
                throw new BadRequestException("You cannot apply to the same listing twice");
            }
        }

        /// <summary>
        /// Gets the Artist and Venue from the ListingApplicationId Associated with it
        /// </summary>
        public async Task<(ArtistDto, VenueDto)> GetArtistAndVenueByIdAsync(int id)
        {
            var (artist, venue) = await listingApplicationRepository.GetArtistAndVenueByIdAsync(id);

            return (mapper.Map<ArtistDto>(artist), mapper.Map<VenueDto>(venue));
        }

        public async Task<decimal> GetListingPayByIdAsync(int id)
        {
            return await listingApplicationRepository.GetListingPayByIdAsync(id);
        }

        public async Task<ListingApplicationDto> GetByIdAsync(int id)
        {
            var application = await listingApplicationRepository.GetByIdAsync(id);

            return mapper.Map<ListingApplicationDto>(application);
        }
    }
}
