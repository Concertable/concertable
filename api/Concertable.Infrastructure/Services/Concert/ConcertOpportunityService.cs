using Core.Entities;
using Application.Interfaces;
using Application.Interfaces.Concert;
using Application.Interfaces.Payment;
using Application.DTOs;
using Core.Exceptions;

namespace Infrastructure.Services.Concert;

public class ConcertOpportunityService : IConcertOpportunityService
{
    private readonly IConcertOpportunityRepository opportunityRepository;
    private readonly IStripeValidator stripeValidator;
    private readonly IVenueService venueService;
    private readonly IConcertOpportunityMapper opportunityMapper;

    public ConcertOpportunityService(
        IConcertOpportunityRepository opportunityRepository,
        IStripeValidator stripeValidator,
        IVenueService venueService,
        IConcertOpportunityMapper opportunityMapper)
    {
        this.opportunityRepository = opportunityRepository;
        this.stripeValidator = stripeValidator;
        this.venueService = venueService;
        this.opportunityMapper = opportunityMapper;
    }

    public async Task CreateAsync(ConcertOpportunityDto opportunityDto)
    {
        var stripeResult = await stripeValidator.ValidateUserAsync();
        if (!stripeResult.IsValid)
            throw new ForbiddenException(stripeResult.Errors.Values.First().First());

        var venueDto = await venueService.GetDetailsForCurrentUserAsync()
            ?? throw new NotFoundException("Venue not found for current user");
        var opportunity = opportunityMapper.ToEntity(opportunityDto);
        opportunity.VenueId = venueDto.Id;

        await opportunityRepository.AddAsync(opportunity);
        await opportunityRepository.SaveChangesAsync();
    }

    public async Task CreateMultipleAsync(IEnumerable<ConcertOpportunityDto> opportunitiesDto)
    {
        var stripeResult = await stripeValidator.ValidateUserAsync();
        if (!stripeResult.IsValid)
            throw new ForbiddenException(stripeResult.Errors.Values.First().First());

        var venueDto = await venueService.GetDetailsForCurrentUserAsync()
            ?? throw new NotFoundException("Venue not found for current user");

        var opportunities = opportunitiesDto.Select(dto =>
        {
            var opportunity = opportunityMapper.ToEntity(dto);
            opportunity.VenueId = venueDto.Id;
            return opportunity;
        }).ToList();

        await opportunityRepository.AddRangeAsync(opportunities);
        await opportunityRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<ConcertOpportunityDto>> GetActiveByVenueIdAsync(int id)
    {
        var opportunities = await opportunityRepository.GetActiveByVenueIdAsync(id);
        return opportunityMapper.ToDtos(opportunities);
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
