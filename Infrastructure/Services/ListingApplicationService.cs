using Core.Entities;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ListingApplicationService : IListingApplicationService
    {
        private readonly IListingApplicationRepository applicationRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAuthService authService;
        private readonly IMessageService messageService;
        private readonly IListingService listingService;
        private readonly IArtistService artistService;

        public ListingApplicationService(
            IListingApplicationRepository applicationRepository,
            IUnitOfWork unitOfWork,
            IAuthService authService,
            IMessageService messageService,
            IListingService listingService,
            IArtistService artistService)
        {
            this.applicationRepository = applicationRepository;
            this.unitOfWork = unitOfWork;
            this.authService = authService;
            this.messageService = messageService;
            this.listingService = listingService;
            this.artistService = artistService;
        }

        public async Task<IEnumerable<ListingApplication>> GetAllForListingIdAsync(int listingId)
        {
            return await applicationRepository.GetAllForListingIdAsync(listingId);
        }

        public async Task ApplyForListingAsync(int listingId)
        {
            // Prepare data for Service calls
            var artistDto = await artistService.GetDetailsForCurrentUserAsync();
            var application = new ListingApplication()
            {
                ListingId = listingId,
                ArtistId = artistDto.Id,
                Approved = false
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
                content: "Test Content");

            // Save changes after both have executed
            await unitOfWork.SaveChangesAsync();
        }
    }
}
