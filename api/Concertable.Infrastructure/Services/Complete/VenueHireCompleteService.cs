using Application.Interfaces.Concert;
using Core.Enums;
using Core.Exceptions;

namespace Infrastructure.Services.Complete;

public class VenueHireCompleteService : ICompleteStrategy
{
    private readonly IConcertApplicationRepository applicationRepository;
    private readonly ISettlementProcessor settlementProcessor;

    public VenueHireCompleteService(
        IConcertApplicationRepository applicationRepository,
        ISettlementProcessor settlementProcessor)
    {
        this.applicationRepository = applicationRepository;
        this.settlementProcessor = settlementProcessor;
    }

    public async Task CompleteAsync(int concertId)
    {
        var application = await applicationRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Application not found");

        application.Status = ApplicationStatus.Complete;
        await applicationRepository.SaveChangesAsync();

        await settlementProcessor.SettleAsync(concertId);
    }
}
