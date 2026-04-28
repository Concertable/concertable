using Concertable.Concert.Application.Responses;
using Concertable.Payment.Application.Interfaces;
using Concertable.Messaging.Contracts;
using Concertable.Shared.Enums;
using Concertable.Shared.Exceptions;
using Concertable.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Services;

internal class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository applicationRepository;
    private readonly ICurrentUser currentUser;
    private readonly IApplicationValidator applicationValidator;
    private readonly IStripeValidator stripeValidator;
    private readonly IMessagingModule messagingModule;
    private readonly IEmailService emailService;
    private readonly IOpportunityService opportunityService;
    private readonly IArtistModule artistModule;
    private readonly IUserModule userModule;
    private readonly IAcceptanceExecutor acceptanceExecutor;
    private readonly IApplicationMapper mapper;

    public ApplicationService(
        IApplicationRepository applicationRepository,
        ICurrentUser currentUser,
        IApplicationValidator applicationValidator,
        IStripeValidator stripeValidator,
        IMessagingModule messagingModule,
        IEmailService emailService,
        IOpportunityService opportunityService,
        IArtistModule artistModule,
        IUserModule userModule,
        IAcceptanceExecutor acceptanceExecutor,
        IApplicationMapper mapper)
    {
        this.applicationRepository = applicationRepository;
        this.currentUser = currentUser;
        this.applicationValidator = applicationValidator;
        this.stripeValidator = stripeValidator;
        this.messagingModule = messagingModule;
        this.emailService = emailService;
        this.opportunityService = opportunityService;
        this.artistModule = artistModule;
        this.userModule = userModule;
        this.acceptanceExecutor = acceptanceExecutor;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<ApplicationDto>> GetByOpportunityIdAsync(int id)
    {
        var response = await opportunityService.OwnsOpportunityAsync(id);

        if (!response)
            throw new ForbiddenException("You do not own this Concert Opportunity");

        var applications = await applicationRepository.GetByOpportunityIdAsync(id);

        return await mapper.ToDtosAsync(applications);
    }

    public async Task<IEnumerable<ApplicationDto>> GetPendingForArtistAsync()
    {
        var artistId = await artistModule.GetIdByUserIdAsync(currentUser.GetId())
            ?? throw new ForbiddenException("You must have an Artist account");
        var applications = await applicationRepository.GetPendingByArtistIdAsync(artistId);
        return await mapper.ToDtosAsync(applications);
    }

    public async Task<IEnumerable<ApplicationDto>> GetRecentDeniedForArtistAsync()
    {
        var artistId = await artistModule.GetIdByUserIdAsync(currentUser.GetId())
            ?? throw new ForbiddenException("You must have an Artist account");
        var applications = await applicationRepository.GetRecentDeniedByArtistIdAsync(artistId);
        return await mapper.ToDtosAsync(applications);
    }

    public async Task<ApplicationDto> ApplyAsync(int opportunityId)
    {
        if (!await stripeValidator.ValidateAccountAsync())
            throw new ForbiddenException("You must have a verified Stripe account to apply for opportunities");

        var artistId = await artistModule.GetIdByUserIdAsync(currentUser.GetId())
            ?? throw new ForbiddenException("You must create an Artist account before you apply for a concert opportunity");

        var application = ApplicationEntity.Create(artistId, opportunityId);

        var opportunityOwnerId = await opportunityService.GetOwnerByIdAsync(opportunityId)
            ?? throw new NotFoundException("Concert Opportunity owner not found");
        var opportunityOwner = await userModule.GetManagerByIdAsync(opportunityOwnerId)
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

        return await mapper.ToDtoAsync(application);
    }

    public Task<AcceptCheckout> CheckoutAsync(int applicationId) =>
        acceptanceExecutor.CheckoutAsync(applicationId);

    public async Task<IAcceptOutcome> AcceptAsync(int applicationId, string? paymentMethodId = null)
    {
        var outcome = await acceptanceExecutor.AcceptAsync(applicationId, paymentMethodId);

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

    public async Task<ApplicationDto> GetByIdAsync(int id)
    {
        var application = await applicationRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Application not found");
        return await mapper.ToDtoAsync(application);
    }
}
