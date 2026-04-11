using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Results;

namespace Concertable.Infrastructure.Validators;

public class OpportunityApplicationValidator : IOpportunityApplicationValidator
{
    private readonly IConcertRepository concertRepository;
    private readonly IOpportunityRepository opportunityRepository;
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly ICurrentUser currentUser;
    private readonly TimeProvider timeProvider;

    public OpportunityApplicationValidator(
        IConcertRepository concertRepository,
        IOpportunityRepository opportunityRepository,
        IOpportunityApplicationRepository applicationRepository,
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
        {
            result.AddError("Concert opportunity does not exist");
            return result;
        }

        if (application is null)
        {
            result.AddError("Concert application does not exist");
            return result;
        }

        if (opportunity.Venue.UserId != currentUser.Get().Id)
            result.AddError("You do not own this concert opportunity");

        if (opportunity.StartDate < timeProvider.GetUtcNow())
            result.AddError("This concert opportunity has already passed");

        if (await concertRepository.OpportunityHasConcertAsync(opportunity.Id))
            result.AddError("This concert opportunity already has a concert booked");

        if (await concertRepository.ArtistHasConcertOnDateAsync(application.ArtistId, opportunity.StartDate))
            result.AddError("This artist already has a concert on this day");

        if (await concertRepository.VenueHasConcertOnDateAsync(opportunity.VenueId, opportunity.StartDate))
            result.AddError("You already have a concert on this day");

        return result;
    }

    public async Task<ValidationResult> CanApplyAsync(int opportunityId, int artistId)
    {
        var result = new ValidationResult();
        var opportunity = await opportunityRepository.GetByIdAsync(opportunityId);

        if (opportunity is null)
        {
            result.AddError("Concert opportunity does not exist");
            return result;
        }

        if (opportunity.StartDate < timeProvider.GetUtcNow())
            result.AddError("This concert opportunity has already passed");

        if (await concertRepository.OpportunityHasConcertAsync(opportunityId))
            result.AddError("This concert opportunity has already been booked for a concert");

        if (await concertRepository.ArtistHasConcertOnDateAsync(artistId, opportunity.StartDate))
            result.AddError("You already have a concert on this day");

        return result;
    }
}
