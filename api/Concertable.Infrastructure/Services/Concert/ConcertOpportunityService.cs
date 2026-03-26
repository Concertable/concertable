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
    private readonly IGenreSyncService genreSyncService;

    public ConcertOpportunityService(
        IConcertOpportunityRepository opportunityRepository,
        IStripeValidator stripeValidator,
        IVenueService venueService,
        IContractService contractService,
        IUnitOfWork unitOfWork,
        IGenreSyncService genreSyncService)
    {
        this.opportunityRepository = opportunityRepository;
        this.stripeValidator = stripeValidator;
        this.venueService = venueService;
        this.contractService = contractService;
        this.unitOfWork = unitOfWork;
        this.genreSyncService = genreSyncService;
    }

    public async Task<ConcertOpportunityDto> CreateAsync(ConcertOpportunityRequest request)
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
            await unitOfWork.TrySaveChangesAsync();

            await contractService.AddAsync(request.Contract, opportunity.Id);
            await unitOfWork.TrySaveChangesAsync();

            await transaction.CommitAsync();

            var saved = await opportunityRepository.GetByIdAsync(opportunity.Id)
                ?? throw new NotFoundException("Opportunity not found after save");

            return saved.ToDto();
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

    public async Task<ConcertOpportunityDto> UpdateAsync(int id, ConcertOpportunityRequest request)
    {
        var stripeResult = await stripeValidator.ValidateUserAsync();
        if (!stripeResult.IsValid)
            throw new ForbiddenException(stripeResult.Errors.First());

        var venueDto = await venueService.GetDetailsForCurrentUserAsync()
            ?? throw new NotFoundException("Venue not found for current user");

        var opportunity = await opportunityRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Concert Opportunity not found");

        if (opportunity.VenueId != venueDto.Id)
            throw new ForbiddenException("You do not own this concert opportunity");

        using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            opportunity.StartDate = request.StartDate;
            opportunity.EndDate = request.EndDate;
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
        return updated.ToDto();
    }

    public async Task<IEnumerable<ConcertOpportunityDto>> GetActiveByVenueIdAsync(int id)
    {
        var opportunities = await opportunityRepository.GetActiveByVenueIdAsync(id);
        return opportunities.ToDtos();
    }

    public async Task<ConcertOpportunityDto> GetByIdAsync(int id)
    {
        var opportunity = await opportunityRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Concert Opportunity not found");
        return opportunity.ToDto();
    }

    public async Task<UserEntity> GetOwnerByIdAsync(int id)
    {
        return await opportunityRepository.GetOwnerByIdAsync(id)
            ?? throw new NotFoundException("Concert Opportunity owner not found");
    }
}
