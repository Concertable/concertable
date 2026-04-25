using System.Transactions;
using Concertable.Payment.Application.Interfaces;
using Concertable.Contract.Abstractions;
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

    public OpportunityService(
        IOpportunityRepository opportunityRepository,
        IStripeValidationFactory stripeValidationFactory,
        IVenueModule venueModule,
        IContractModule contractModule,
        IOpportunityMapper mapper,
        ICurrentUser currentUser)
    {
        this.opportunityRepository = opportunityRepository;
        this.stripeValidationFactory = stripeValidationFactory;
        this.venueModule = venueModule;
        this.contractModule = contractModule;
        this.mapper = mapper;
        this.currentUser = currentUser;
    }

    public async Task<OpportunityDto> CreateAsync(OpportunityRequest request)
    {
        if (!await stripeValidationFactory.Create(request.Contract.ContractType).ValidateAsync())
            throw new ForbiddenException("You do not have the required Stripe account set up");

        var venueId = await venueModule.GetVenueIdByUserIdAsync(currentUser.GetId())
            ?? throw new NotFoundException("Venue not found for current user");

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var contractId = await contractModule.CreateAsync(request.Contract);
        var opportunity = OpportunityEntity.Create(
            venueId,
            new DateRange(request.StartDate, request.EndDate),
            contractId,
            request.GenreIds);

        await opportunityRepository.AddAsync(opportunity);
        await opportunityRepository.SaveChangesAsync();

        scope.Complete();

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

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

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

        await opportunityRepository.SaveChangesAsync();
        scope.Complete();
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

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await contractModule.UpdateAsync(opportunity.ContractId, request.Contract);
            opportunity.Update(
                new DateRange(request.StartDate, request.EndDate),
                opportunity.ContractId,
                request.GenreIds);
            await opportunityRepository.SaveChangesAsync();
            scope.Complete();
        }

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
