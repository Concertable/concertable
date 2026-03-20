using Application.Interfaces.Concert;
using Core.Enums;
using Core.Exceptions;

namespace Infrastructure.Services.Accept;

public class FlatFeeAcceptService : IAcceptStrategy
{
    private readonly IConcertApplicationValidator applicationValidator;
    private readonly IConcertApplicationRepository applicationRepository;

    public FlatFeeAcceptService(
        IConcertApplicationValidator applicationValidator,
        IConcertApplicationRepository applicationRepository)
    {
        this.applicationValidator = applicationValidator;
        this.applicationRepository = applicationRepository;
    }

    public async Task AcceptAsync(int applicationId)
    {
        var result = await applicationValidator.CanAcceptAsync(applicationId);

        if (!result.IsValid)
            throw new BadRequestException(result.Errors);

        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        application.Status = ApplicationStatus.AwaitingPayment;

        await applicationRepository.SaveChangesAsync();
    }
}
