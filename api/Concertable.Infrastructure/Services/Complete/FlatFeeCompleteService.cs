using Application.Interfaces.Concert;
using Core.Enums;
using Core.Exceptions;

namespace Infrastructure.Services.Complete;

public class FlatFeeCompleteService : ICompleteStrategy
{
    private readonly IConcertApplicationRepository applicationRepository;

    public FlatFeeCompleteService(IConcertApplicationRepository applicationRepository)
    {
        this.applicationRepository = applicationRepository;
    }

    public async Task CompleteAsync(int concertId)
    {
        var application = await applicationRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Application not found");

        application.Status = ApplicationStatus.Complete;
        await applicationRepository.SaveChangesAsync();
    }
}
