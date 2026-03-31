using Concertable.Core.Entities;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.DTOs;
using Concertable.Application.Mappers;
using Concertable.Core.Exceptions;
using Concertable.Core.Enums;

namespace Concertable.Infrastructure.Services.Concert;

public class ConcertApplicationService : IConcertApplicationService
{
    private readonly IConcertApplicationRepository applicationRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ICurrentUser currentUser;
    private readonly IConcertApplicationValidator applicationValidator;
    private readonly IStripeValidator stripeValidator;
    private readonly IMessageService messageService;
    private readonly IEmailService emailService;
    private readonly IConcertOpportunityService opportunityService;
    private readonly IArtistService artistService;
    private readonly IOwnershipService ownershipService;
    private readonly IAcceptProcessor acceptProcessor;
    private readonly IConcertApplicationMapper mapper;

    public ConcertApplicationService(
        IConcertApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IConcertApplicationValidator applicationValidator,
        IStripeValidator stripeValidator,
        IMessageService messageService,
        IEmailService emailService,
        IConcertOpportunityService opportunityService,
        IOwnershipService ownershipService,
        IArtistService artistService,
        IAcceptProcessor acceptProcessor,
        IConcertApplicationMapper mapper)
    {
        this.applicationRepository = applicationRepository;
        this.unitOfWork = unitOfWork;
        this.currentUser = currentUser;
        this.applicationValidator = applicationValidator;
        this.stripeValidator = stripeValidator;
        this.messageService = messageService;
        this.emailService = emailService;
        this.opportunityService = opportunityService;
        this.artistService = artistService;
        this.ownershipService = ownershipService;
        this.acceptProcessor = acceptProcessor;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<ConcertApplicationDto>> GetByOpportunityIdAsync(int id)
    {
        var response = await ownershipService.OwnsOpportunityAsync(id);

        if (!response)
            throw new ForbiddenException("You do not own this Concert Opportunity");

        var applications = await applicationRepository.GetByOpportunityIdAsync(id);

        return mapper.ToDtos(applications);
    }

    public async Task<IEnumerable<ConcertApplicationDto>> GetPendingForArtistAsync()
    {
        var artistId = await artistService.GetIdForCurrentUserAsync();
        var applications = await applicationRepository.GetPendingByArtistIdAsync(artistId);
        return mapper.ToDtos(applications);
    }

    public async Task<IEnumerable<ConcertApplicationDto>> GetRecentDeniedForArtistAsync()
    {
        var artistId = await artistService.GetIdForCurrentUserAsync();
        var applications = await applicationRepository.GetRecentDeniedByArtistIdAsync(artistId);
        return mapper.ToDtos(applications);
    }

    public async Task<ConcertApplicationDto> ApplyAsync(int opportunityId)
    {
        var stripeResult = await stripeValidator.ValidateUserAsync();
        if (!stripeResult.IsValid)
            throw new ForbiddenException(stripeResult.Errors.First());

        var artistDto = await artistService.GetDetailsForCurrentUserAsync()
            ?? throw new ForbiddenException("You must create an Artist account before you apply for a concert opportunity");

        var application = new ConcertApplicationEntity()
        {
            OpportunityId = opportunityId,
            ArtistId = artistDto.Id,
        };

        var user = currentUser.Get();
        var opportunityOwner = await opportunityService.GetOwnerByIdAsync(opportunityId);
        var opportunity = await opportunityService.GetByIdAsync(opportunityId);

        var result = await applicationValidator.CanApplyAsync(opportunityId, artistDto.Id);

        if (!result.IsValid)
            throw new BadRequestException(result.Errors);

        var artistGenreIds = artistDto.Genres.Select(g => g.Id).ToHashSet();
        var opportunityGenreIds = opportunity.Genres.Select(g => g.Id).ToHashSet();

        if (opportunityGenreIds.Count > 0 && !artistGenreIds.Overlaps(opportunityGenreIds))
            throw new BadRequestException("You need to have the same genres as the Concert Opportunity to be able to apply to it");

        await applicationRepository.AddAsync(application);

        await messageService.SendAsync(
            fromUserId: user.Id,
            toUserId: opportunityOwner.Id,
            content: $"{user.Email} has applied to your concert opportunity",
            action: "application",
            actionId: opportunityId);

        await emailService.SendEmailAsync(opportunityOwner.Email!, "Concert Application", $"{user.Email} has applied to your concert opportunity");

        try
        {
            await unitOfWork.TrySaveChangesAsync();
        }
        catch (BadRequestException ex) when (ex.ErrorType == ErrorType.DuplicateKey)
        {
            throw new BadRequestException("You cannot apply to the same concert opportunity twice");
        }

        return mapper.ToDto(application);
    }

    public async Task AcceptAsync(int applicationId)
    {
        await acceptProcessor.AcceptAsync(applicationId);

        var (artist, venue) = await applicationRepository.GetArtistAndVenueByIdAsync(applicationId)
            ?? throw new NotFoundException("Concert application not found");

        await messageService.SendAndSaveAsync(
            venue.UserId,
            artist.UserId,
            "application",
            applicationId,
            "Your application has been accepted!");

        await emailService.SendEmailAsync(artist.User.Email!, "Concert Application Accepted", "Your application was accepted! A concert has been scheduled for you.");
    }

    public async Task<(ArtistDto, VenueDto)> GetArtistAndVenueByIdAsync(int id)
    {
        var (artist, venue) = await applicationRepository.GetArtistAndVenueByIdAsync(id)
            ?? throw new NotFoundException("Concert application not found");
        return (artist.ToDto(), venue.ToDto());
    }

    public async Task<ConcertApplicationDto> GetByIdAsync(int id)
    {
        var application = await applicationRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Application not found");
        return mapper.ToDto(application);
    }

}
