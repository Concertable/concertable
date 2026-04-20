using Concertable.Core.Entities;
using Concertable.Application.Interfaces;
using Concertable.Identity.Contracts;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.DTOs;
using Concertable.Application.Mappers;
using Concertable.Application.Responses;
using Concertable.Core.Enums;
using Concertable.Application.Exceptions;

namespace Concertable.Infrastructure.Services.Concert;

public class OpportunityApplicationService : IOpportunityApplicationService
{
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ICurrentUser currentUser;
    private readonly IOpportunityApplicationValidator applicationValidator;
    private readonly IStripeValidator stripeValidator;
    private readonly IMessageService messageService;
    private readonly IEmailService emailService;
    private readonly IOpportunityService opportunityService;
    private readonly IArtistService artistService;
    private readonly IOwnershipService ownershipService;
    private readonly IAcceptDispatcher acceptDispatcher;
    private readonly IOpportunityApplicationMapper mapper;

    public OpportunityApplicationService(
        IOpportunityApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IOpportunityApplicationValidator applicationValidator,
        IStripeValidator stripeValidator,
        IMessageService messageService,
        IEmailService emailService,
        IOpportunityService opportunityService,
        IOwnershipService ownershipService,
        IArtistService artistService,
        IAcceptDispatcher acceptDispatcher,
        IOpportunityApplicationMapper mapper)
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
        this.acceptDispatcher = acceptDispatcher;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<OpportunityApplicationDto>> GetByOpportunityIdAsync(int id)
    {
        var response = await ownershipService.OwnsOpportunityAsync(id);

        if (!response)
            throw new ForbiddenException("You do not own this Concert Opportunity");

        var applications = await applicationRepository.GetByOpportunityIdAsync(id);

        return mapper.ToDtos(applications);
    }

    public async Task<IEnumerable<OpportunityApplicationDto>> GetPendingForArtistAsync()
    {
        var artistId = await artistService.GetIdForCurrentUserAsync();
        var applications = await applicationRepository.GetPendingByArtistIdAsync(artistId);
        return mapper.ToDtos(applications);
    }

    public async Task<IEnumerable<OpportunityApplicationDto>> GetRecentDeniedForArtistAsync()
    {
        var artistId = await artistService.GetIdForCurrentUserAsync();
        var applications = await applicationRepository.GetRecentDeniedByArtistIdAsync(artistId);
        return mapper.ToDtos(applications);
    }

    public async Task<OpportunityApplicationDto> ApplyAsync(int opportunityId)
    {
        if (!await stripeValidator.ValidateAccountAsync())
            throw new ForbiddenException("You must have a verified Stripe account to apply for opportunities");

        var artistDto = await artistService.GetDetailsForCurrentUserAsync()
            ?? throw new ForbiddenException("You must create an Artist account before you apply for a concert opportunity");

        var application = OpportunityApplicationEntity.Create(artistDto.Id, opportunityId);

        var opportunityOwner = await opportunityService.GetOwnerByIdAsync(opportunityId);
        var opportunity = await opportunityService.GetByIdAsync(opportunityId);

        var result = await applicationValidator.CanApplyAsync(opportunityId, artistDto.Id);

        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        var artistGenreIds = artistDto.Genres.Select(g => g.Id).ToHashSet();
        var opportunityGenreIds = opportunity.Genres.Select(g => g.Id).ToHashSet();

        if (opportunityGenreIds.Count > 0 && !artistGenreIds.Overlaps(opportunityGenreIds))
            throw new BadRequestException("You need to have the same genres as the Concert Opportunity to be able to apply to it");

        await applicationRepository.AddAsync(application);

        await messageService.SendAsync(
            fromUserId: currentUser.GetId(),
            toUserId: opportunityOwner.Id,
            content: $"{currentUser.Email} has applied to your concert opportunity",
            action: MessageAction.ApplicationReceived);

        await emailService.SendEmailAsync(opportunityOwner.Email!, "Concert Application", $"{currentUser.Email} has applied to your concert opportunity");

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

    public async Task<IAcceptOutcome> AcceptAsync(int applicationId, string? paymentMethodId = null)
    {
        var outcome = await acceptDispatcher.AcceptAsync(applicationId, paymentMethodId);

        var (artist, venue) = await applicationRepository.GetArtistAndVenueByIdAsync(applicationId)
            ?? throw new NotFoundException("Concert application not found");

        await messageService.SendAndSaveAsync(
            fromUserId: venue.UserId,
            toUserId: artist.UserId,
            content: "Your application has been accepted!",
            action: MessageAction.ApplicationAccepted);

        await emailService.SendEmailAsync(artist.Email!, "Concert Application Accepted", "Your application was accepted! A concert has been scheduled for you.");

        return outcome;
    }

    public async Task<(ArtistDto, VenueDto)> GetArtistAndVenueByIdAsync(int id)
    {
        var (artist, venue) = await applicationRepository.GetArtistAndVenueByIdAsync(id)
            ?? throw new NotFoundException("Concert application not found");
        return (artist.ToDto(), venue.ToDto());
    }

    public async Task<OpportunityApplicationDto> GetByIdAsync(int id)
    {
        var application = await applicationRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Application not found");
        return mapper.ToDto(application);
    }

}
