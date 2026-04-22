using Concertable.Application.Interfaces.Payment;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services;

internal class OpportunityService : IOpportunityService
{
    private readonly IOpportunityRepository opportunityRepository;
    private readonly IStripeValidationFactory stripeValidationFactory;
    private readonly IVenueModule venueModule;
    private readonly IContractMapper contractMapper;
    private readonly IUnitOfWork unitOfWork;
    private readonly IOpportunityMapper mapper;
    private readonly ICurrentUser currentUser;

    public OpportunityService(
        IOpportunityRepository opportunityRepository,
        IStripeValidationFactory stripeValidationFactory,
        IVenueModule venueModule,
        IContractMapper contractMapper,
        IUnitOfWork unitOfWork,
        IOpportunityMapper mapper,
        ICurrentUser currentUser)
    {
        this.opportunityRepository = opportunityRepository;
        this.stripeValidationFactory = stripeValidationFactory;
        this.venueModule = venueModule;
        this.contractMapper = contractMapper;
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
        this.currentUser = currentUser;
    }

    public async Task<OpportunityDto> CreateAsync(OpportunityRequest request)
    {
        if (!await stripeValidationFactory.Create(request.Contract.ContractType).ValidateAsync())
            throw new ForbiddenException("You do not have the required Stripe account set up");

        var venueId = await venueModule.GetVenueIdByUserIdAsync(currentUser.GetId())
            ?? throw new NotFoundException("Venue not found for current user");

        using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var contract = contractMapper.ToEntity(request.Contract);
            var opportunity = OpportunityEntity.Create(
                venueId,
                new DateRange(request.StartDate, request.EndDate),
                contract,
                request.GenreIds);

            await opportunityRepository.AddAsync(opportunity);
            await unitOfWork.TrySaveChangesAsync();
            await transaction.CommitAsync();

            var saved = await opportunityRepository.GetByIdAsync(opportunity.Id)
                ?? throw new NotFoundException("Opportunity not found after save");

            return mapper.ToDto(saved);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
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

        using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            foreach (var request in requests)
            {
                var contract = contractMapper.ToEntity(request.Contract);
                var opportunity = OpportunityEntity.Create(
                    venueId,
                    new DateRange(request.StartDate, request.EndDate),
                    contract,
                    request.GenreIds);

                await opportunityRepository.AddAsync(opportunity);
            }

            await unitOfWork.TrySaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
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

        using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var contract = contractMapper.ToEntity(request.Contract);
            opportunity.Update(new DateRange(request.StartDate, request.EndDate), contract, request.GenreIds);
            await unitOfWork.TrySaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
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
