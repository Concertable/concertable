using Concertable.Core.Entities;
using Concertable.Application.Interfaces;
using Concertable.Core.ValueObjects;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.DTOs;
using Concertable.Application.Mappers;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Concertable.Application.Exceptions;
using Concertable.Core.Interfaces;

namespace Concertable.Infrastructure.Services.Concert;

public class OpportunityService : IOpportunityService
{
    private readonly IOpportunityRepository opportunityRepository;
    private readonly IStripeValidationFactory stripeValidationFactory;
    private readonly IVenueService venueService;
    private readonly IContractService contractService;
    private readonly IUnitOfWork unitOfWork;
    private readonly IGenreSyncService genreSyncService;
    private readonly IOpportunityMapper mapper;

    public OpportunityService(
        IOpportunityRepository opportunityRepository,
        IStripeValidationFactory stripeValidationFactory,
        IVenueService venueService,
        IContractService contractService,
        IUnitOfWork unitOfWork,
        IGenreSyncService genreSyncService,
        IOpportunityMapper mapper)
    {
        this.opportunityRepository = opportunityRepository;
        this.stripeValidationFactory = stripeValidationFactory;
        this.venueService = venueService;
        this.contractService = contractService;
        this.unitOfWork = unitOfWork;
        this.genreSyncService = genreSyncService;
        this.mapper = mapper;
    }

    public async Task<OpportunityDto> CreateAsync(OpportunityRequest request)
    {
        if (!await stripeValidationFactory.Create(request.Contract.ContractType).ValidateAsync())
            throw new ForbiddenException("You do not have the required Stripe account set up");

        var venueDto = await venueService.GetDetailsForCurrentUserAsync()
            ?? throw new NotFoundException("Venue not found for current user");

        using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var opportunity = request.ToEntity();
            opportunity.VenueId = venueDto.Id;

            await opportunityRepository.AddAsync(opportunity);
            await unitOfWork.TrySaveChangesAsync();

            await contractService.AddAsync(request.Contract, opportunity.Id);
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

        var venueDto = await venueService.GetDetailsForCurrentUserAsync()
            ?? throw new NotFoundException("Venue not found for current user");

        using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            foreach (var request in requests)
            {
                var opportunity = request.ToEntity();
                opportunity.VenueId = venueDto.Id;

                await opportunityRepository.AddAsync(opportunity);
                await unitOfWork.TrySaveChangesAsync();

                await contractService.AddAsync(request.Contract, opportunity.Id);
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

        var venueDto = await venueService.GetDetailsForCurrentUserAsync()
            ?? throw new NotFoundException("Venue not found for current user");

        var opportunity = await opportunityRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Concert Opportunity not found");

        if (opportunity.VenueId != venueDto.Id)
            throw new ForbiddenException("You do not own this concert opportunity");

        using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            opportunity.Period = new DateRange(request.StartDate, request.EndDate);
            genreSyncService.Sync(opportunity.OpportunityGenres, request.GenreIds);
            await contractService.UpdateAsync(request.Contract, id);
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

    public async Task<UserEntity> GetOwnerByIdAsync(int id)
    {
        return await opportunityRepository.GetOwnerByIdAsync(id)
            ?? throw new NotFoundException("Concert Opportunity owner not found");
    }
}
