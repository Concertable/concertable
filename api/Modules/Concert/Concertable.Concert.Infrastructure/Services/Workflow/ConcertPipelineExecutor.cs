using Concertable.Concert.Application.Responses;
using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Application.Workflow.Steps;
using Concertable.Shared.Exceptions;
using Concertable.Shared.Infrastructure.Extensions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class ConcertPipelineExecutor :
    IApplyDispatcher,
    ICheckoutDispatcher,
    IAcceptanceDispatcher,
    ISettlementDispatcher,
    ICompletionDispatcher,
    IVerifyDispatcher
{
    private readonly IContractLoader contractLoader;
    private readonly IConcertLifecycleRepository lifecycleRepository;
    private readonly IConcertPipelineFactory pipelineFactory;
    private readonly IConcertStateMachineFactory stateMachineFactory;
    private readonly IApplicationRepository applicationRepository;
    private readonly ILogger<ConcertPipelineExecutor> logger;

    public ConcertPipelineExecutor(
        IContractLoader contractLoader,
        IConcertLifecycleRepository lifecycleRepository,
        IConcertPipelineFactory pipelineFactory,
        IConcertStateMachineFactory stateMachineFactory,
        IApplicationRepository applicationRepository,
        ILogger<ConcertPipelineExecutor> logger)
    {
        this.contractLoader = contractLoader;
        this.lifecycleRepository = lifecycleRepository;
        this.pipelineFactory = pipelineFactory;
        this.stateMachineFactory = stateMachineFactory;
        this.applicationRepository = applicationRepository;
        this.logger = logger;
    }

    public async Task<ApplicationEntity> ApplyAsync(int opportunityId, int artistId)
    {
        var contract = await contractLoader.LoadByOpportunityIdAsync(opportunityId);
        var lifecycleId = await GetOrCreateLifecycleAsync(opportunityId, artistId);
        var stateMachine = stateMachineFactory.Create(contract.ContractType);

        var step = pipelineFactory.Create<ISimpleApplyStep>(contract.ContractType)
            ?? throw new BadRequestException("This contract requires a payment method at apply");

        await stateMachine.GuardAsync<ISimpleApplyStep>(lifecycleId);
        var app = await step.ExecuteAsync(artistId, opportunityId);
        app.SetLifecycleId(lifecycleId);
        await PersistApplicationAsync(app);
        await stateMachine.AdvanceAsync<ISimpleApplyStep>(lifecycleId);
        return app;
    }

    public async Task<ApplicationEntity> ApplyAsync(int opportunityId, int artistId, string paymentMethodId)
    {
        var contract = await contractLoader.LoadByOpportunityIdAsync(opportunityId);
        var lifecycleId = await lifecycleRepository.GetIdByOpportunityIdAndArtistIdAsync(opportunityId, artistId)
            ?? throw new BadRequestException("Checkout must be completed before applying");
        var stateMachine = stateMachineFactory.Create(contract.ContractType);

        var step = pipelineFactory.Create<IPaidApplyStep>(contract.ContractType)
            ?? throw new BadRequestException("This contract does not accept a payment method at apply");

        await stateMachine.GuardAsync<IPaidApplyStep>(lifecycleId);
        var app = await step.ExecuteAsync(artistId, opportunityId, paymentMethodId);
        app.SetLifecycleId(lifecycleId);
        await PersistApplicationAsync(app);
        await stateMachine.AdvanceAsync<IPaidApplyStep>(lifecycleId);
        return app;
    }

    public async Task<Checkout> ApplyCheckoutAsync(int opportunityId)
    {
        var contract = await contractLoader.LoadByOpportunityIdAsync(opportunityId);

        var step = pipelineFactory.Create<IApplyCheckoutStep>(contract.ContractType)
            ?? throw new BadRequestException("This contract does not support a pre-apply checkout");

        return await step.ExecuteAsync(opportunityId);
    }

    public async Task<Checkout> AcceptCheckoutAsync(int applicationId)
    {
        var contract = await contractLoader.LoadByApplicationIdAsync(applicationId);
        var lifecycleId = await lifecycleRepository.GetIdByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Concert lifecycle not found for application");
        var stateMachine = stateMachineFactory.Create(contract.ContractType);

        var step = pipelineFactory.Create<IAcceptCheckoutStep>(contract.ContractType)
            ?? throw new BadRequestException("This contract does not support an accept checkout");

        await stateMachine.GuardAsync<IAcceptCheckoutStep>(lifecycleId);
        var checkout = await step.ExecuteAsync(applicationId);
        await stateMachine.AdvanceAsync<IAcceptCheckoutStep>(lifecycleId);
        return checkout;
    }

    public async Task AcceptAsync(int applicationId, string? paymentMethodId)
    {
        var contract = await contractLoader.LoadByApplicationIdAsync(applicationId);
        var lifecycleId = await lifecycleRepository.GetIdByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Concert lifecycle not found for application");
        var stateMachine = stateMachineFactory.Create(contract.ContractType);

        if (paymentMethodId is not null)
        {
            var step = pipelineFactory.Create<IPaidAcceptStep>(contract.ContractType)
                ?? throw new BadRequestException("This contract does not accept a payment method at acceptance");
            await stateMachine.GuardAsync<IPaidAcceptStep>(lifecycleId);
            await step.ExecuteAsync(applicationId, paymentMethodId);
            await stateMachine.AdvanceAsync<IPaidAcceptStep>(lifecycleId);
        }
        else
        {
            var step = pipelineFactory.Create<ISimpleAcceptStep>(contract.ContractType)
                ?? throw new BadRequestException("This contract requires a payment method at acceptance");
            await stateMachine.GuardAsync<ISimpleAcceptStep>(lifecycleId);
            await step.ExecuteAsync(applicationId);
            await stateMachine.AdvanceAsync<ISimpleAcceptStep>(lifecycleId);
        }
    }

    public async Task VerifyAsync(int applicationId)
    {
        var contract = await contractLoader.LoadByApplicationIdAsync(applicationId);
        var lifecycleId = await lifecycleRepository.GetIdByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Concert lifecycle not found for application");
        var stateMachine = stateMachineFactory.Create(contract.ContractType);

        var step = pipelineFactory.Create<IVerifyStep>(contract.ContractType)
            ?? throw new BadRequestException("This contract type does not support payment verification");

        await stateMachine.GuardAsync<IVerifyStep>(lifecycleId);
        await step.ExecuteAsync(applicationId);
        await stateMachine.AdvanceAsync<IVerifyStep>(lifecycleId);
    }

    public async Task SettleAsync(int bookingId)
    {
        var contract = await contractLoader.TryLoadByBookingIdAsync(bookingId);
        if (contract is null)
            return;

        logger.LogDebug("Dispatching settlement for booking {BookingId} ({ContractType})", bookingId, contract.ContractType);

        var lifecycleId = await lifecycleRepository.GetIdByBookingIdAsync(bookingId)
            ?? throw new NotFoundException("Concert lifecycle not found for booking");
        var stateMachine = stateMachineFactory.Create(contract.ContractType);

        var step = pipelineFactory.Create<ISettleStep>(contract.ContractType)
            ?? throw new BadRequestException("This contract does not support settlement");

        await stateMachine.GuardAsync<ISettleStep>(lifecycleId);
        await step.ExecuteAsync(bookingId);
        await stateMachine.AdvanceAsync<ISettleStep>(lifecycleId);
    }

    public async Task<Result> FinishAsync(int concertId)
    {
        try
        {
            var contract = await contractLoader.LoadByConcertIdAsync(concertId);
            var lifecycleId = await lifecycleRepository.GetIdByConcertIdAsync(concertId)
                ?? throw new NotFoundException("Concert lifecycle not found for concert");
            var stateMachine = stateMachineFactory.Create(contract.ContractType);

            var step = pipelineFactory.Create<IFinishStep>(contract.ContractType)
                ?? throw new BadRequestException("This contract does not support finish");

            await stateMachine.GuardAsync<IFinishStep>(lifecycleId);
            await step.ExecuteAsync(concertId);
            await stateMachine.AdvanceAsync<IFinishStep>(lifecycleId);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to finish concert {ConcertId}", concertId);
            return Result.Fail(ex.Message);
        }
    }

    private async Task<int> GetOrCreateLifecycleAsync(int opportunityId, int artistId)
    {
        var existing = await lifecycleRepository.GetIdByOpportunityIdAndArtistIdAsync(opportunityId, artistId);
        if (existing.HasValue)
            return existing.Value;

        var lifecycle = ConcertLifecycleEntity.Create(opportunityId, artistId);
        await lifecycleRepository.AddAsync(lifecycle);
        await lifecycleRepository.SaveChangesAsync();
        return lifecycle.Id;
    }

    private async Task PersistApplicationAsync(ApplicationEntity app)
    {
        await applicationRepository.AddAsync(app);
        try
        {
            await applicationRepository.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsDuplicateKey())
        {
            throw new BadRequestException("You cannot apply to the same concert opportunity twice");
        }
    }
}
