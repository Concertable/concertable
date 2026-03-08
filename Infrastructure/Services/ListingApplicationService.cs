using Core.Entities;
using Application.Interfaces;
using Application.DTOs;
using Application.Mappers;
using Core.Exceptions;
using Core.Enums;
using Infrastructure.Repositories;

namespace Infrastructure.Services
{
    public class ListingApplicationService : IListingApplicationService
    {
        private readonly IListingApplicationRepository listingApplicationRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentUserService currentUserService;
        private readonly IListingApplicationValidationService applicationValidationService;
        private readonly IStripeValidationService stripeValidationService;
        private readonly IMessageService messageService;
        private readonly IEmailService emailService;
        private readonly IListingService listingService;
        private readonly IArtistService artistService;
        private readonly IOwnershipService ownershipService;

        public ListingApplicationService(
            IListingApplicationRepository listingApplicationRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IListingApplicationValidationService applicationValidationService,
            IStripeValidationService stripeValidationService,
            IMessageService messageService,
            IEmailService emailService,
            IListingService listingService,
            IOwnershipService ownershipService,
            IArtistService artistService)
        {
            this.listingApplicationRepository = listingApplicationRepository;
            this.unitOfWork = unitOfWork;
            this.currentUserService = currentUserService;
            this.applicationValidationService = applicationValidationService;
            this.stripeValidationService = stripeValidationService;
            this.messageService = messageService;
            this.emailService = emailService;
            this.listingService = listingService;
            this.artistService = artistService;
            this.ownershipService = ownershipService;
        }

        public async Task<IEnumerable<ListingApplicationDto>> GetForListingIdAsync(int id)
        {
            var response = await ownershipService.OwnsListingAsync(id);

            if (!response)
                throw new ForbiddenException("You do not own this Listing");

            var applications = await listingApplicationRepository.GetByListingIdAsync(id);

            return applications.ToDtos();
        }

        public async Task<IEnumerable<ArtistListingApplicationDto>> GetPendingForArtistAsync()
        {
            var artistId = await artistService.GetIdForCurrentUserAsync();
            var applications = await listingApplicationRepository.GetPendingByArtistIdAsync(artistId);
            return applications.ToArtistListingApplicationDtos();
        }

        public async Task<IEnumerable<ArtistListingApplicationDto>> GetRecentDeniedForArtistAsync()
        {
            var artistId = await artistService.GetIdForCurrentUserAsync();
            var applications = await listingApplicationRepository.GetRecentDeniedByArtistIdAsync(artistId);
            return applications.ToArtistListingApplicationDtos();
        }

        public async Task ApplyForListingAsync(int listingId)
        {
            await stripeValidationService.ValidateUserAsync();

            var artistDto = await artistService.GetDetailsForCurrentUserAsync();

            if (artistDto is null)
                throw new ForbiddenException("You must create an Artist account before you apply for a listing");

            var application = new ListingApplication()
            {
                ListingId = listingId,
                ArtistId = artistDto.Id,
            };

            var user = await currentUserService.GetAsync();
            var listingOwner = await listingService.GetOwnerByIdAsync(listingId);
            var listing = await listingService.GetByIdAsync(listingId);

            var response = await applicationValidationService.CanApplyForListingAsync(listingId, artistDto.Id);

            if (!response.IsValid)
                throw new BadRequestException(response.Reason!);

            var artistGenreIds = artistDto.Genres.Select(g => g.Id).ToHashSet();
            var listingGenreIds = listing.ListingGenres.Select(lg => lg.GenreId).ToHashSet();

            if (listingGenreIds.Count > 0 && !artistGenreIds.Overlaps(listingGenreIds))
                throw new BadRequestException("You need to have the same genres as the Listing to be able to apply to it");

            var applicationRepository = unitOfWork.GetRepository<ListingApplication>();
            await applicationRepository.AddAsync(application);

            await messageService.SendAsync(
                fromUserId: user.Id,
                toUserId: listingOwner.Id,
                content: $"{user.Email} has applied to your listing",
                action: "application",
                actionId: listingId);

            await emailService.SendEmailAsync(listingOwner.Email!, "Listing Application", $"{user.Email} has applied to your listing");

            try
            {
                await unitOfWork.TrySaveChangesAsync();
            }
            catch (BadRequestException ex) when (ex.ErrorType == ErrorType.DuplicateKey)
            {
                throw new BadRequestException("You cannot apply to the same listing twice");
            }
        }

        public async Task<(ArtistDto, VenueDto)> GetArtistAndVenueByIdAsync(int id)
        {
            var (artist, venue) = await listingApplicationRepository.GetArtistAndVenueByIdAsync(id);
            return (artist.ToDto(), venue.ToDto());
        }

        public async Task<decimal> GetListingPayByIdAsync(int id)
        {
            return await listingApplicationRepository.GetListingPayByIdAsync(id);
        }

        public async Task<ListingApplicationDto> GetByIdAsync(int id)
        {
            var application = await listingApplicationRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Application not found");
            return application.ToDto();
        }
    }
}
