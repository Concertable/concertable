using Concertable.Payment.Application.Interfaces;
using Concertable.Contract.Contracts;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services;

internal class OpportunityService : IOpportunityService
{
    private readonly IOpportunityRepository opportunityRepository;
    private readonly IStripeValidationFactory stripeValidationFactory;
    private readonly IVenueModule venueModule;
    private readonly IContractModule contractModule;
    private readonly IOpportunityMapper mapper;
    private readonly ICurrentUser currentUser;
    private readonly IUnitOfWorkBehavior uowBehavior;

    public OpportunityService(
        IOpportunityRepository opportunityRepository,
        IStripeValidationFactory stripeValidationFactory,
        IVenueModule venueModule,
        IContractModule contractModule,
        IOpportunityMapper mapper,
        ICurrentUser currentUser,
        IUnitOfWorkBehavior uowBehavior)
    {
        this.opportunityRepository = opportunityRepository;
        this.stripeValidationFactory = stripeValidationFactory;
        this.venueModule = venueModule;
        this.contractModule = contractModule;
        this.mapper = mapper;
        this.currentUser = currentUser;
        this.uowBehavior = uowBehavior;
    }

    public async Task<OpportunityDto> CreateAsync(OpportunityRequest request)
    {
        if (!await stripeValidationFactory.Create(request.Contract.ContractType).ValidateAsync())
            throw new ForbiddenException("You do not have the required Stripe account set up");

        var venueId = await venueModule.GetVenueIdByUserIdAsync(currentUser.GetId())
            ?? throw new NotFoundException("Venue not found for current user");

        var opportunity = await uowBehavior.ExecuteAsync(async () =>
        {
            var contractId = await contractModule.CreateAsync(request.Contract);
            var entity = OpportunityEntity.Create(
                venueId,
                new DateRange(request.StartDate, request.EndDate),
                contractId,
                request.GenreIds);
            await opportunityRepository.AddAsync(entity);
            return entity;
        });

        var saved = await opportunityRepository.GetByIdAsync(opportunity.Id)
            ?? throw new NotFoundException("Opportunity not found after save");
        return mapper.ToDto(saved);
    }

    public async Task CreateMultipleAsync(IEnumerable<OpportunityRequest> requests)
    {
        var requestList = requests.ToList();
        foreach (var request in requestList)
        {
            if (!await stripeValidationFactory.Create(request.Contract.ContractType).ValidateAsync())
                throw new ForbiddenException("You do not have the required Stripe account set up");
        }

        var venueId = await venueModule.GetVenueIdByUserIdAsync(currentUser.GetId())
            ?? throw new NotFoundException("Venue not found for current user");

        await uowBehavior.ExecuteAsync(async () =>
        {
            foreach (var request in requestList)
            {
                var contractId = await contractModule.CreateAsync(request.Contract);
                var opportunity = OpportunityEntity.Create(
                    venueId,
                    new DateRange(request.StartDate, request.EndDate),
                    contractId,
                    request.GenreIds);
                await opportunityRepository.AddAsync(opportunity);
            }
        });
    }

    public async Task<OpportunityDto> UpdateAsync(int id, OpportunityRequest request)
    {
        if (!await stripeValidationFactory.Create(request.Contract.ContractType).ValidateAsync())
            throw new ForbiddenException("You do not have the required Stripe account set up");

        var venueId = await venueModule.GetVenueIdByUserIdAsync(currentUser.GetId())
            ?? throw new NotFoundException("Venue not found for current user");

        var opportunity = await opportunityRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Concert Opportunity not found");

        if (opportunity.VenueId != venueId)
            throw new ForbiddenException("You do not own this concert opportunity");

        await uowBehavior.ExecuteAsync(async () =>
        {
            await contractModule.UpdateAsync(opportunity.ContractId, request.Contract);
            opportunity.Update(
                new DateRange(request.StartDate, request.EndDate),
                opportunity.ContractId,
                request.GenreIds);
        });

        var updated = await opportunityRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Concert Opportunity not found");
        return mapper.ToDto(updated);
    }

    public async Task<IPagination<OpportunityDto>> GetActiveByVenueIdAsync(int id, IPageParams pageParams)
    {
        var opportunities = await opportunityRepository.GetActiveByVenueIdAsync(id, pageParams);
        return mapper.ToDtos(opportunities);
    }

    public async Task<OpportunityDto> GetByIdAsync(int id)
    {
        var opportunity = await opportunityRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Concert Opportunity not found");
        return mapper.ToDto(opportunity);
    }

    public async Task<Guid?> GetOwnerByIdAsync(int id)
    {
        return await opportunityRepository.GetOwnerByIdAsync(id);
    }

    public async Task<bool> OwnsOpportunityAsync(int opportunityId)
    {
        var opportunity = await opportunityRepository.GetWithVenueByIdAsync(opportunityId);
        return opportunity?.Venue?.UserId == currentUser.GetId();
    }

    public async Task<bool> OwnsOpportunityByApplicationIdAsync(int applicationId)
    {
        var opportunity = await opportunityRepository.GetByApplicationIdAsync(applicationId);
        return opportunity?.Venue?.UserId == currentUser.GetId();
    }
}
