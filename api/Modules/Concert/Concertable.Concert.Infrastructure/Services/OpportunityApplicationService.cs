using Concertable.Concert.Application.Responses;
using Concertable.Payment.Application.Interfaces;
using Concertable.Messaging.Contracts;
using Concertable.Shared.Enums;
using Concertable.Shared.Exceptions;
using Concertable.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Services;

internal class OpportunityApplicationService : IOpportunityApplicationService
{
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly ICurrentUser currentUser;
    private readonly IOpportunityApplicationValidator applicationValidator;
    private readonly IStripeValidator stripeValidator;
    private readonly IMessagingModule messagingModule;
    private readonly IEmailService emailService;
    private readonly IOpportunityService opportunityService;
    private readonly IArtistModule artistModule;
    private readonly IManagerModule managerModule;
    private readonly IAcceptanceDispatcher acceptDispatcher;
    private readonly IOpportunityApplicationMapper mapper;

    public OpportunityApplicationService(
        IOpportunityApplicationRepository applicationRepository,
        ICurrentUser currentUser,
        IOpportunityApplicationValidator applicationValidator,
        IStripeValidator stripeValidator,
        IMessagingModule messagingModule,
        IEmailService emailService,
        IOpportunityService opportunityService,
        IArtistModule artistModule,
        IManagerModule managerModule,
        IAcceptanceDispatcher acceptDispatcher,
        IOpportunityApplicationMapper mapper)
    {
        this.applicationRepository = applicationRepository;
        this.currentUser = currentUser;
        this.applicationValidator = applicationValidator;
        this.stripeValidator = stripeValidator;
        this.messagingModule = messagingModule;
        this.emailService = emailService;
        this.opportunityService = opportunityService;
        this.artistModule = artistModule;
        this.managerModule = managerModule;
        this.acceptDispatcher = acceptDispatcher;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<OpportunityApplicationDto>> GetByOpportunityIdAsync(int id)
    {
        var response = await opportunityService.OwnsOpportunityAsync(id);

        if (!response)
            throw new ForbiddenException("You do not own this Concert Opportunity");

        var applications = await applicationRepository.GetByOpportunityIdAsync(id);

        return mapper.ToDtos(applications);
    }

    public async Task<IEnumerable<OpportunityApplicationDto>> GetPendingForArtistAsync()
    {
        var artistId = await artistModule.GetIdByUserIdAsync(currentUser.GetId())
            ?? throw new ForbiddenException("You must have an Artist account");
        var applications = await applicationRepository.GetPendingByArtistIdAsync(artistId);
        return mapper.ToDtos(applications);
    }

    public async Task<IEnumerable<OpportunityApplicationDto>> GetRecentDeniedForArtistAsync()
    {
        var artistId = await artistModule.GetIdByUserIdAsync(currentUser.GetId())
            ?? throw new ForbiddenException("You must have an Artist account");
        var applications = await applicationRepository.GetRecentDeniedByArtistIdAsync(artistId);
        return mapper.ToDtos(applications);
    }

    public async Task<OpportunityApplicationDto> ApplyAsync(int opportunityId)
    {
        if (!await stripeValidator.ValidateAccountAsync())
            throw new ForbiddenException("You must have a verified Stripe account to apply for opportunities");

        var artistId = await artistModule.GetIdByUserIdAsync(currentUser.GetId())
            ?? throw new ForbiddenException("You must create an Artist account before you apply for a concert opportunity");

        var application = OpportunityApplicationEntity.Create(artistId, opportunityId);

        var opportunityOwnerId = await opportunityService.GetOwnerByIdAsync(opportunityId)
            ?? throw new NotFoundException("Concert Opportunity owner not found");
        var opportunityOwner = await managerModule.GetByIdAsync(opportunityOwnerId)
            ?? throw new NotFoundException("Venue manager not found for opportunity owner");
        var opportunity = await opportunityService.GetByIdAsync(opportunityId);

        var result = await applicationValidator.CanApplyAsync(opportunityId, artistId);

        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        var artistGenreIds = await artistModule.GetGenreIdsAsync(artistId);
        var opportunityGenreIds = opportunity.Genres.Select(g => g.Id).ToHashSet();

        if (opportunityGenreIds.Count > 0 && !artistGenreIds.Overlaps(opportunityGenreIds))
            throw new BadRequestException("You need to have the same genres as the Concert Opportunity to be able to apply to it");

        await applicationRepository.AddAsync(application);

        await messagingModule.SendAsync(
            fromUserId: currentUser.GetId(),
            toUserId: opportunityOwner.Id,
            content: $"{currentUser.Email} has applied to your concert opportunity",
            action: MessageAction.ApplicationReceived);

        await emailService.SendEmailAsync(opportunityOwner.Email!, "Concert Application", $"{currentUser.Email} has applied to your concert opportunity");

        try
        {
            await applicationRepository.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsDuplicateKey())
        {
            throw new BadRequestException("You cannot apply to the same concert opportunity twice");
        }

        return mapper.ToDto(application);
    }

    public Task<AcceptPreview> GetAcceptPreviewAsync(int applicationId) =>
        acceptDispatcher.PreviewAsync(applicationId);

    public async Task<IAcceptOutcome> AcceptAsync(int applicationId, string? paymentMethodId = null)
    {
        var outcome = await acceptDispatcher.AcceptAsync(applicationId, paymentMethodId);

        var (artist, venue) = await applicationRepository.GetArtistAndVenueByIdAsync(applicationId)
            ?? throw new NotFoundException("Concert application not found");

        await messagingModule.SendAndNotifyAsync(
            fromUserId: venue.UserId,
            toUserId: artist.UserId,
            content: "Your application has been accepted!",
            action: MessageAction.ApplicationAccepted);

        await emailService.SendEmailAsync(artist.Email!, "Concert Application Accepted", "Your application was accepted! A concert has been scheduled for you.");

        return outcome;
    }

    public async Task<(ArtistReadModel, VenueReadModel)?> GetArtistAndVenueByIdAsync(int id) =>
        await applicationRepository.GetArtistAndVenueByIdAsync(id);

    public async Task<OpportunityApplicationDto> GetByIdAsync(int id)
    {
        var application = await applicationRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Application not found");
        return mapper.ToDto(application);
    }
}
