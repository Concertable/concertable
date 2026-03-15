using Application.Interfaces;
using Application.Interfaces.Concert;
using Application.Responses;

namespace Infrastructure.Validators;

public class ConcertApplicationValidator : IConcertApplicationValidator
{
    private readonly IConcertRepository concertRepository;
    private readonly IConcertOpportunityRepository opportunityRepository;
    private readonly IConcertApplicationRepository applicationRepository;
    private readonly ICurrentUser currentUser;
    private readonly TimeProvider timeProvider;

    public ConcertApplicationValidator(
        IConcertRepository concertRepository,
        IConcertOpportunityRepository opportunityRepository,
        IConcertApplicationRepository applicationRepository,
        ICurrentUser currentUser,
        TimeProvider timeProvider)
    {
        this.concertRepository = concertRepository;
        this.opportunityRepository = opportunityRepository;
        this.applicationRepository = applicationRepository;
        this.currentUser = currentUser;
        this.timeProvider = timeProvider;
    }

    public async Task<ValidationResult> CanAcceptAsync(int applicationId)
    {
        var result = new ValidationResult();
        var opportunity = await opportunityRepository.GetByApplicationIdAsync(applicationId);
        var application = await applicationRepository.GetByIdAsync(applicationId);

        if (opportunity is null)
            return result.AddError("Concert Opportunity does not exist");

        if (application is null)
            return result.AddError("Concert Application does not exist");

        var userId = currentUser.Get().Id;
        if (opportunity.Venue.UserId != userId)
            return result.AddError("You do not own this Concert Opportunity");

        if (opportunity.StartDate < timeProvider.GetUtcNow())
            return result.AddError("You can't accept this application because the concert opportunity has already passed");

        if (await concertRepository.OpportunityHasConcertAsync(opportunity.Id))
            return result.AddError("This concert opportunity already has a concert booked");

        if (await concertRepository.ArtistHasConcertOnDateAsync(application.ArtistId, opportunity.StartDate))
            return result.AddError("This artist already has a concert on this day");

        if (await concertRepository.VenueHasConcertOnDateAsync(opportunity.VenueId, opportunity.StartDate))
            return result.AddError("You already have a concert on this day");

        return result;
    }

    public async Task<ValidationResult> CanApplyAsync(int opportunityId, int artistId)
    {
        var result = new ValidationResult();
        var opportunity = await opportunityRepository.GetByIdAsync(opportunityId);

        if (opportunity is null)
            return result.AddError("Concert Opportunity does not exist.");

        if (opportunity.StartDate < timeProvider.GetUtcNow())
            return result.AddError("This concert opportunity has already passed.");

        if (await concertRepository.OpportunityHasConcertAsync(opportunityId))
            return result.AddError("This concert opportunity has already been booked for a concert.");

        if (await concertRepository.ArtistHasConcertOnDateAsync(artistId, opportunity.StartDate))
            return result.AddError("You already have a concert on this day.");

        return result;
    }
}
