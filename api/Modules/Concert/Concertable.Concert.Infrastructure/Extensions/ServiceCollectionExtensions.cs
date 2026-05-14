using Concertable.Application.Interfaces;
using Concertable.Artist.Contracts.Events;
using Concertable.Concert.Application.Interfaces.Reviews;
using Concertable.Concert.Application.Mappers;
using Concertable.Concert.Application.Validators;
using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Application.Workflow.Executors;
using Concertable.Concert.Infrastructure.Services.Workflow.Dispatchers;
using Concertable.Concert.Infrastructure.Services.Workflow.Executors;
using Concertable.Concert.Infrastructure.Services.Workflow.Steps;
using Concertable.Concert.Infrastructure.Services.Workflow.Workflows;
using Concertable.Concert.Contracts;
using Concertable.Concert.Contracts.Events;
using Concertable.Concert.Domain.Events;
using Concertable.Concert.Infrastructure.Data;
using Concertable.Concert.Infrastructure.Data.Seeders;
using Concertable.Concert.Infrastructure.Events;
using Concertable.Concert.Infrastructure.Handlers;
using Concertable.Concert.Infrastructure.Repositories;
using Concertable.Concert.Infrastructure.Repositories.Review;
using Concertable.Concert.Infrastructure.Services;
using Concertable.Concert.Infrastructure.Services.Workflow;
using Concertable.Concert.Infrastructure.Services.Completion;
using Concertable.Concert.Infrastructure.Services.Payment;
using Concertable.Concert.Infrastructure.Services.Review;
using Concertable.Concert.Infrastructure.Validators;
using Concertable.Data.Infrastructure.Data;
using Concertable.Payment.Contracts;
using Concertable.Payment.Contracts.Events;
using Concertable.Shared;
using Concertable.Venue.Contracts.Events;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConcertModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ConcertDbContext>((sp, opts) =>
            opts.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.UseNetTopologySuite())
                .AddInterceptors(
                    sp.GetRequiredService<AuditInterceptor>(),
                    sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddScoped<IUnitOfWork<ConcertDbContext>, UnitOfWork<ConcertDbContext>>();
        services.AddScoped<IUnitOfWorkBehavior, UnitOfWorkBehavior>();

        // Services
        services.AddScoped<IConcertService, ConcertService>();
        services.AddScoped<IConcertDraftService, ConcertDraftService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IConcertNotifier, ConcertNotifier>();
        services.AddScoped<ITicketNotifier, TicketNotifier>();
        services.AddScoped<IOpportunityService, OpportunityService>();
        services.AddScoped<IOpportunitySyncer>(sp => new Sync.OpportunitySyncer(
            sp.GetRequiredService<IOpportunityRepository>(),
            sp.GetRequiredService<IContractModule>()));
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IImmediateConcertService, ImmediateConcertService>();
        services.AddScoped<IDeferredConcertService, DeferredConcertService>();
        services.AddScoped<IContractLoader, ContractLoader>();
        services.AddScoped<IPayerLookup, PayerLookup>();
        services.AddScoped<ITicketService, TicketService>();

        // Review service + validator (Concert owns reviews; Artist/Venue lists/can-review go through IConcertModule facade)
        services.AddScoped<IConcertReviewService, ConcertReviewService>();
        services.AddScoped<IReviewValidator, ReviewValidator>();

        // Business-rule validators (interfaces in Concert.Application, impls in Concert.Infrastructure.Validators)
        services.AddSingleton<IConcertValidator, ConcertValidator>();
        services.AddScoped<ITicketValidator, TicketValidator>();
        services.AddScoped<IApplicationValidator, ApplicationValidator>();

        // Ticket QR + PDF (Concert-owned, were in legacy Concertable.Infrastructure)
        services.AddSingleton<QRCoder.QRCodeGenerator>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<IPdfService, PdfService>();

        services.AddScoped<IConcertCompletionRunner, ConcertCompletionRunner>();

        services.AddScoped<IConcertTransitionValidatorFactory, ConcertTransitionValidatorFactory>();
        services.AddScoped<IConcertWorkflowFactory, ConcertWorkflowFactory>();
        services.AddScoped<IConcertWorkflowCapabilityRegistry, ConcertWorkflowCapabilityRegistry>();
        services.AddScoped(typeof(ILifecycleRepository<>), typeof(LifecycleRepository<>));
        services.AddScoped(typeof(IWorkflowStateMachine<>), typeof(WorkflowStateMachine<>));

        services.AddScoped<IApplyExecutor, ApplyExecutor>();
        services.AddScoped<IAcceptExecutor, AcceptExecutor>();
        services.AddScoped<IVerifyExecutor, VerifyExecutor>();
        services.AddScoped<ISettleExecutor, SettleExecutor>();
        services.AddScoped<IFinishExecutor, FinishExecutor>();

        services.AddScoped<IApplyDispatcher, ApplyDispatcher>();
        services.AddScoped<IAcceptanceDispatcher, AcceptanceDispatcher>();
        services.AddScoped<ICheckoutDispatcher, CheckoutDispatcher>();
        services.AddScoped<IVerifyDispatcher, VerifyDispatcher>();
        services.AddScoped<ISettlementDispatcher, SettlementDispatcher>();
        services.AddScoped<ICompletionDispatcher, CompletionDispatcher>();

        services.AddConcertPipeline(ContractType.FlatFee, p => p
            .WithSimpleApply<SimpleApplyStep>()
            .WithAcceptCheckout<FlatFeeAcceptCheckoutStep>()
            .WithSimpleAccept<FlatFeeAcceptStep>()
            .WithSettle<HeldSettleStep>()
            .WithFinish<FlatFeeFinishStep>()
            .WithWorkflow<FlatFeeWorkflow>());

        services.AddConcertPipeline(ContractType.DoorSplit, p => p
            .WithSimpleApply<SimpleApplyStep>()
            .WithAcceptCheckout<DoorSplitAcceptCheckoutStep>()
            .WithVerify<DeferredVerifyStep>()
            .WithPaidAccept<DoorSplitAcceptStep>()
            .WithSettle<DeferredSettleStep>()
            .WithFinish<DoorSplitFinishStep>()
            .WithWorkflow<DoorSplitWorkflow>());

        services.AddConcertPipeline(ContractType.Versus, p => p
            .WithSimpleApply<SimpleApplyStep>()
            .WithAcceptCheckout<VersusAcceptCheckoutStep>()
            .WithVerify<DeferredVerifyStep>()
            .WithPaidAccept<VersusAcceptStep>()
            .WithSettle<DeferredSettleStep>()
            .WithFinish<VersusFinishStep>()
            .WithWorkflow<VersusWorkflow>());

        services.AddConcertPipeline(ContractType.VenueHire, p => p
            .WithApplyCheckout<VenueHireApplyCheckoutStep>()
            .WithPaidApply<PaidApplyStep>()
            .WithSimpleAccept<VenueHireAcceptStep>()
            .WithSettle<ApplyCommittedSettleStep>()
            .WithFinish<VenueHireFinishStep>()
            .WithWorkflow<VenueHireWorkflow>());

        // Ticket payee â€” composite dispatches by ContractType to artist vs venue
        services.AddSingleton<ArtistTicketPayee>();
        services.AddSingleton<VenueTicketPayee>();
        services.AddSingleton<ITicketPayee, TicketPayee>();

        // Repositories
        services.AddScoped<IConcertRepository, ConcertRepository>();
        services.AddScoped<IOpportunityRepository, OpportunityRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IArtistReviewRepository, ArtistReviewRepository>();
        services.AddScoped<IVenueReviewRepository, VenueReviewRepository>();
        services.AddScoped<IConcertReviewRepository, ConcertReviewRepository>();

        // Mappers
        services.AddScoped<IOpportunityMapper, OpportunityMapper>();
        services.AddScoped<IApplicationMapper, ApplicationMapper>();

        // Module facades
        services.AddScoped<IConcertModule, ConcertModule>();
        services.AddScoped<IConcertWorkflowModule, ConcertWorkflowModule>();

        // Domain event â†’ integration event + read-model projection handlers
        services.AddScoped<IDomainEventHandler<ReviewCreatedDomainEvent>, ReviewCreatedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<ApplicationAcceptedDomainEvent>, ApplicationAcceptedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<BookingSettledDomainEvent>, BookingSettledDomainEventHandler>();
        services.AddScoped<IIntegrationEventHandler<ArtistChangedEvent>, ArtistReadModelProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<VenueChangedEvent>, VenueReadModelProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<ReviewSubmittedEvent>, ConcertReviewProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<PaymentSucceededEvent>, PaymentSucceededEventHandler>();
        services.AddScoped<IPaymentSucceededProcessorFactory, PaymentSucceededProcessorFactory>();
        services.AddKeyedScoped<IPaymentSucceededProcessor, TicketPaymentProcessor>(TransactionTypes.Ticket);
        services.AddKeyedScoped<IPaymentSucceededProcessor, SettlementPaymentProcessor>(TransactionTypes.Settlement);
        services.AddKeyedScoped<IPaymentSucceededProcessor, EscrowPaymentProcessor>(TransactionTypes.Escrow);
        services.AddKeyedScoped<IPaymentSucceededProcessor, VerifyPaymentProcessor>(TransactionTypes.Verify);

        services.AddSingleton<ConcertConfigurationProvider>();
        services.AddSingleton<IEntityTypeConfigurationProvider>(sp => sp.GetRequiredService<ConcertConfigurationProvider>());
        services.AddSingleton<IRatingProjectionConfigurationProvider, ConcertRatingProjectionConfigurationProvider>();

        services.AddValidatorsFromAssemblyContaining<OpportunityDtoValidator>();

        return services;
    }

    public static IServiceCollection AddConcertDevSeeder(this IServiceCollection services)
    {
        services.AddScoped<IDevSeeder, ConcertDevSeeder>();
        return services;
    }

    public static IServiceCollection AddConcertTestSeeder(this IServiceCollection services)
    {
        services.AddScoped<ITestSeeder, ConcertTestSeeder>();
        return services;
    }

}
