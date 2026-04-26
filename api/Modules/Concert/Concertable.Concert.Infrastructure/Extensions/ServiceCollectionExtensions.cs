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
using Concertable.Concert.Infrastructure.Services.Acceptance;
using Concertable.Concert.Infrastructure.Services.Application;
using Concertable.Concert.Infrastructure.Services.Completion;
using Concertable.Concert.Infrastructure.Services.Payment;
using Concertable.Concert.Infrastructure.Services.Review;
using Concertable.Concert.Infrastructure.Services.Settlement;
using Concertable.Concert.Infrastructure.Validators;
using Concertable.Data.Infrastructure.Data;
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

        // Services
        services.AddScoped<IConcertService, ConcertService>();
        services.AddScoped<IConcertDraftService, ConcertDraftService>();
        services.AddScoped<IConcertNotifier, ConcertNotifier>();
        services.AddScoped<ITicketNotifier, TicketNotifier>();
        services.AddScoped<IOpportunityService, OpportunityService>();
        services.AddScoped<IOpportunityApplicationService, OpportunityApplicationService>();
        services.AddScoped<IUpfrontConcertService, UpfrontConcertService>();
        services.AddScoped<IDeferredConcertService, DeferredConcertService>();
        services.AddScoped<IContractLookup, ContractLookup>();
        services.AddScoped<ITicketService, TicketService>();

        // Review service + validator (Concert owns reviews; Artist/Venue lists/can-review go through IConcertModule facade)
        services.AddScoped<IConcertReviewService, ConcertReviewService>();
        services.AddScoped<IReviewValidator, ReviewValidator>();

        // Business-rule validators (interfaces in Concert.Application, impls in Concert.Infrastructure.Validators)
        services.AddSingleton<IConcertValidator, ConcertValidator>();
        services.AddScoped<ITicketValidator, TicketValidator>();
        services.AddScoped<IOpportunityApplicationValidator, OpportunityApplicationValidator>();

        // Ticket QR + PDF (Concert-owned, were in legacy Concertable.Infrastructure)
        services.AddSingleton<QRCoder.QRCodeGenerator>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<IPdfService, PdfService>();

        // Dispatchers
        services.AddScoped<IAcceptanceDispatcher, AcceptanceDispatcher>();
        services.AddScoped<ICompletionDispatcher, CompletionDispatcher>();
        services.AddScoped<ISettlementDispatcher, SettlementDispatcher>();
        services.AddScoped<IApplicationAcceptHandler, ApplicationAcceptHandler>();

        // Keyed workflow strategies (keys must match ContractType enum values exactly)
        services.AddKeyedScoped<IConcertWorkflowStrategy, FlatFeeConcertWorkflow>(ContractType.FlatFee);
        services.AddKeyedScoped<IConcertWorkflowStrategy, DoorSplitConcertWorkflow>(ContractType.DoorSplit);
        services.AddKeyedScoped<IConcertWorkflowStrategy, VersusConcertWorkflow>(ContractType.Versus);
        services.AddKeyedScoped<IConcertWorkflowStrategy, VenueHireConcertWorkflow>(ContractType.VenueHire);
        services.AddScoped<IConcertWorkflowStrategyFactory, ConcertWorkflowStrategyFactory>();

        // Ticket payee — composite dispatches by ContractType to artist vs venue
        services.AddSingleton<ArtistTicketPayee>();
        services.AddSingleton<VenueTicketPayee>();
        services.AddSingleton<ITicketPayee, TicketPayee>();

        // Repositories
        services.AddScoped<IConcertRepository, ConcertRepository>();
        services.AddScoped<IOpportunityRepository, OpportunityRepository>();
        services.AddScoped<IOpportunityApplicationRepository, OpportunityApplicationRepository>();
        services.AddScoped<IConcertBookingRepository, ConcertBookingRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IArtistReviewRepository, ArtistReviewRepository>();
        services.AddScoped<IVenueReviewRepository, VenueReviewRepository>();
        services.AddScoped<IConcertReviewRepository, ConcertReviewRepository>();

        // Mappers (Concert.Application singletons)
        services.AddSingleton<IOpportunityMapper, OpportunityMapper>();
        services.AddSingleton<IOpportunityApplicationMapper, OpportunityApplicationMapper>();

        // Module facades
        services.AddScoped<IConcertModule, ConcertModule>();
        services.AddScoped<IConcertWorkflowModule, ConcertWorkflowModule>();

        // Domain event → integration event + read-model projection handlers
        services.AddScoped<IDomainEventHandler<ReviewCreatedDomainEvent>, ReviewCreatedDomainEventHandler>();
        services.AddScoped<IIntegrationEventHandler<ArtistChangedEvent>, ArtistReadModelProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<VenueChangedEvent>, VenueReadModelProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<ReviewSubmittedEvent>, ConcertReviewProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<PaymentSucceededEvent>, PaymentSucceededEventHandler>();
        services.AddScoped<IPaymentSucceededStrategyFactory, PaymentSucceededStrategyFactory>();
        services.AddKeyedScoped<IPaymentSucceededStrategy, TicketPaymentService>("ticket");
        services.AddKeyedScoped<IPaymentSucceededStrategy, SettlementPaymentService>("settlement");

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
