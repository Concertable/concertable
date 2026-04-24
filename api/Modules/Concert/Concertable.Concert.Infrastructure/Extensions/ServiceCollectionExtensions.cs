using Concertable.Application.Interfaces;
using Concertable.Artist.Contracts.Events;
using Concertable.Concert.Application.Interfaces.Reviews;
using Concertable.Concert.Application.Mappers;
using Concertable.Concert.Application.Validators;
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
using Concertable.Concert.Infrastructure.Services.Accept;
using Concertable.Concert.Infrastructure.Services.Application;
using Concertable.Concert.Infrastructure.Services.Complete;
using Concertable.Concert.Infrastructure.Services.Review;
using Concertable.Concert.Infrastructure.Services.Settlement;
using Concertable.Concert.Infrastructure.Services.Webhook;
using Concertable.Concert.Infrastructure.Validators;
using Concertable.Data.Infrastructure.Data;
using Concertable.Infrastructure.Factories;
using Concertable.Infrastructure.Handlers;
using Concertable.Infrastructure.Interfaces;
using Concertable.Infrastructure.Services.Payment;
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

        // Services
        services.AddScoped<IConcertService, ConcertService>();
        services.AddScoped<IConcertDraftService, ConcertDraftService>();
        services.AddScoped<IOpportunityService, OpportunityService>();
        services.AddScoped<IOpportunityApplicationService, OpportunityApplicationService>();
        services.AddScoped<IContractService, ContractService>();
        services.AddScoped<IUpfrontConcertService, UpfrontConcertService>();
        services.AddScoped<IDeferredConcertService, DeferredConcertService>();

        // Review service + validator (Concert owns reviews; Artist/Venue lists/can-review go through IConcertModule facade)
        services.AddScoped<IConcertReviewService, ConcertReviewService>();
        services.AddScoped<IReviewValidator, ReviewValidator>();

        // Dispatchers
        services.AddScoped<IAcceptDispatcher, AcceptDispatcher>();
        services.AddScoped<IFinishedDispatcher, FinishedDispatcher>();
        services.AddScoped<ISettlementDispatcher, SettlementDispatcher>();
        // TEMPORARY: TicketPaymentDispatcher + ApplicationAcceptHandler still in legacy
        // Concertable.Infrastructure until Payment extraction moves them.
        services.AddScoped<ITicketPaymentDispatcher, TicketPaymentDispatcher>();
        services.AddScoped<IApplicationAcceptHandler, ApplicationAcceptHandler>();

        // Keyed workflow strategies (keys must match ContractType enum values exactly)
        services.AddKeyedScoped<IConcertWorkflowStrategy, FlatFeeConcertWorkflow>(ContractType.FlatFee);
        services.AddKeyedScoped<IConcertWorkflowStrategy, DoorSplitConcertWorkflow>(ContractType.DoorSplit);
        services.AddKeyedScoped<IConcertWorkflowStrategy, VersusConcertWorkflow>(ContractType.Versus);
        services.AddKeyedScoped<IConcertWorkflowStrategy, VenueHireConcertWorkflow>(ContractType.VenueHire);

        // Contract strategy factory/resolver (impls still in legacy Infrastructure — TEMPORARY)
        services.AddScoped(typeof(IContractStrategyFactory<>), typeof(ContractStrategyFactory<>));
        services.AddScoped(typeof(IContractStrategyResolver<>), typeof(ContractStrategyResolver<>));

        // Webhook plumbing (WebhookStrategyFactory still legacy — stays in Web AddContracts)
        services.AddScoped<IWebhookProcessor, WebhookProcessor>();
        services.AddScoped<IWebhookQueue, WebhookQueue>();
        services.AddKeyedScoped<IWebhookStrategy, TicketWebhookHandler>(WebhookType.Concert);
        services.AddKeyedScoped<IWebhookStrategy, SettlementWebhookHandler>(WebhookType.Settlement);

        // Repositories
        services.AddScoped<IConcertRepository, ConcertRepository>();
        services.AddScoped<IOpportunityRepository, OpportunityRepository>();
        services.AddScoped<IOpportunityApplicationRepository, OpportunityApplicationRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IConcertBookingRepository, ConcertBookingRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IArtistReviewRepository, ArtistReviewRepository>();
        services.AddScoped<IVenueReviewRepository, VenueReviewRepository>();
        services.AddScoped<IConcertReviewRepository, ConcertReviewRepository>();

        // Mappers (Concert.Application singletons)
        services.AddSingleton<IContractMapper, ContractMapper>();
        services.AddSingleton<IOpportunityMapper, OpportunityMapper>();
        services.AddSingleton<IOpportunityApplicationMapper, OpportunityApplicationMapper>();

        // Module facade
        services.AddScoped<IConcertModule, ConcertModule>();

        // Domain event → integration event + read-model projection handlers
        services.AddScoped<IDomainEventHandler<ReviewCreatedDomainEvent>, ReviewCreatedDomainEventHandler>();
        services.AddScoped<IIntegrationEventHandler<ArtistChangedEvent>, ArtistReadModelProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<VenueChangedEvent>, VenueReadModelProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<ReviewSubmittedEvent>, ConcertReviewProjectionHandler>();

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
