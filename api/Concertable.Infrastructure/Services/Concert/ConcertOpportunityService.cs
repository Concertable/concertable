using Core.Entities;
using Application.Interfaces;
using Application.Interfaces.Concert;
using Application.Mappers;
using Application.Interfaces.Payment;
using Application.DTOs;
using Application.Requests;
using Core.Exceptions;

namespace Infrastructure.Services.Concert;

public class ConcertOpportunityService : IConcertOpportunityService
{
    private readonly IConcertOpportunityRepository opportunityRepository;
    private readonly IStripeValidator stripeValidator;
    private readonly IVenueService venueService;
    private readonly IContractService contractService;
    private readonly IUnitOfWork unitOfWork;

    public ConcertOpportunityService(
        IConcertOpportunityRepository opportunityRepository,
        IStripeValidator stripeValidator,
        IVenueService venueService,
        IContractService contractService,
        IUnitOfWork unitOfWork)
    {
        this.opportunityRepository = opportunityRepository;
        this.stripeValidator = stripeValidator;
        this.venueService = venueService;
        this.contractService = contractService;
        this.unitOfWork = unitOfWork;
    }

    public async Task CreateAsync(ConcertOpportunityRequest request)
    {
        var stripeResult = await stripeValidator.ValidateUserAsync();
        if (!stripeResult.IsValid)
            throw new ForbiddenException(stripeResult.Errors.First());

        var venueDto = await venueService.GetDetailsForCurrentUserAsync()
            ?? throw new NotFoundException("Venue not found for current user");

        using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var opportunity = request.ToEntity();
            opportunity.VenueId = venueDto.Id;

            await opportunityRepository.AddAsync(opportunity);
            await contractService.AddAsync(request.Contract, opportunity.Id);
            await unitOfWork.TrySaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task CreateMultipleAsync(IEnumerable<ConcertOpportunityRequest> requests)
    {
        var stripeResult = await stripeValidator.ValidateUserAsync();
        if (!stripeResult.IsValid)
            throw new ForbiddenException(stripeResult.Errors.First());

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

    public async Task<IEnumerable<ConcertOpportunityDto>> GetActiveByVenueIdAsync(int id)
    {
        var opportunities = await opportunityRepository.GetActiveByVenueIdAsync(id);
        return opportunities.ToDtos();
    }

    public async Task<ConcertOpportunityEntity> GetByIdAsync(int id)
    {
        return await opportunityRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Concert Opportunity not found");
    }

    public async Task<UserEntity> GetOwnerByIdAsync(int id)
    {
        return await opportunityRepository.GetOwnerByIdAsync(id)
            ?? throw new NotFoundException("Concert Opportunity owner not found");
    }
}
