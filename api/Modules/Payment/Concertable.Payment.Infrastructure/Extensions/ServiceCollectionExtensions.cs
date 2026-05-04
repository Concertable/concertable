using Concertable.Application.Interfaces;
using Concertable.User.Contracts;
using Concertable.User.Contracts.Events;
using Concertable.Payment.Application.Interfaces;
using Concertable.Payment.Contracts;
using Concertable.Payment.Contracts.Events;
using Concertable.Data.Infrastructure;
using Concertable.Data.Infrastructure.Data;
using Concertable.Payment.Infrastructure.Data;
using Concertable.Payment.Infrastructure.Data.Seeders;
using Concertable.Payment.Infrastructure.Events;
using Concertable.Payment.Infrastructure.Handlers;
using Concertable.Payment.Infrastructure.Repositories;
using Concertable.Payment.Infrastructure.Services;
using Concertable.Payment.Infrastructure.Services.Webhook;
using Concertable.Payment.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Payment.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PaymentDbContext>((sp, opts) =>
            opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                .AddInterceptors(
                    sp.GetRequiredService<AuditInterceptor>(),
                    sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddSingleton<PaymentConfigurationProvider>();
        services.AddSingleton<IEntityTypeConfigurationProvider>(sp => sp.GetRequiredService<PaymentConfigurationProvider>());

        services.Configure<StripeSettings>(configuration.GetSection("Stripe"));

        // Repositories + mappers
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IStripeEventRepository, StripeEventRepository>();
        services.AddScoped<IPayoutAccountRepository, PayoutAccountRepository>();
        services.AddScoped<IEscrowRepository, EscrowRepository>();
        services.AddSingleton<ITransactionMapper, TransactionMapper>();

        // Transaction service
        services.AddScoped<ITransactionService, TransactionService>();

        // Stripe real/fake toggle
        var useRealStripe = configuration.GetSection("ExternalServices").GetValue<bool>("UseRealStripe");
        if (useRealStripe)
        {
            services.AddSingleton<Stripe.AccountService>();
            services.AddSingleton<Stripe.AccountLinkService>();
            services.AddSingleton<Stripe.CustomerService>();
            services.AddSingleton<Stripe.PaymentMethodService>();
            services.AddSingleton<Stripe.SetupIntentService>();
            services.AddSingleton<Stripe.PaymentIntentService>();
            services.AddSingleton<Stripe.CustomerSessionService>();
            services.AddScoped<IStripeAccountClient, StripeAccountClient>();
            services.AddSingleton<IStripePaymentClient, StripePaymentClient>();
            services.AddKeyedScoped<IStripePaymentIntentClient, OnSessionStripePaymentIntentClient>(PaymentSession.OnSession);
            services.AddKeyedScoped<IStripePaymentIntentClient, OffSessionStripePaymentIntentClient>(PaymentSession.OffSession);
            services.AddScoped<IWebhookService, WebhookService>();
        }
        else
        {
            services.AddScoped<IStripeAccountClient, FakeStripeAccountClient>();
            services.AddKeyedScoped<IStripePaymentIntentClient, FakeStripePaymentIntentClient>(PaymentSession.OnSession);
            services.AddKeyedScoped<IStripePaymentIntentClient, FakeStripePaymentIntentClient>(PaymentSession.OffSession);
            services.AddScoped<IWebhookService, FakeWebhookService>();
        }

        services.AddScoped<IStripePaymentIntentClientFactory, StripePaymentIntentClientFactory>();
        services.AddScoped<IPaymentManager, PaymentManager>();

        // Stripe validation (keyed by ContractType) â€” used by Concert eligibility checks
        services.AddScoped<StripeAccountValidator>();
        services.AddScoped<StripeCustomerValidator>();
        services.AddScoped<IStripeValidator, StripeValidator>();
        services.AddScoped<IStripeValidationFactory, StripeValidationFactory>();
        services.AddKeyedScoped<IStripeValidationStrategy, StripeAccountValidator>(ContractType.VenueHire);
        services.AddKeyedScoped<IStripeValidationStrategy, StripeCustomerValidator>(ContractType.FlatFee);
        services.AddKeyedScoped<IStripeValidationStrategy, StripeCustomerValidator>(ContractType.DoorSplit);
        services.AddKeyedScoped<IStripeValidationStrategy, StripeCustomerValidator>(ContractType.Versus);

        // Webhook infrastructure
        services.AddScoped<IWebhookProcessor, WebhookProcessor>();
        services.AddScoped<IWebhookQueue, WebhookQueue>();

        // Module facades â€” public Payment.Contracts surface
        services.AddScoped<ICustomerPaymentModule, CustomerPaymentModule>();
        services.AddScoped<IManagerPaymentModule, ManagerPaymentModule>();

        // Integration event handlers
        services.AddScoped<IIntegrationEventHandler<UserRegisteredEvent>, UserRegisteredHandler>();
        services.AddScoped<IIntegrationEventHandler<PaymentSucceededEvent>, PaymentTransactionHandler>();
        services.AddScoped<IIntegrationEventHandler<PaymentFailedEvent>, PaymentFailureDispatcher>();
        services.AddScoped<ITransactionHandlerFactory, TransactionHandlerFactory>();
        services.AddScoped<IPaymentFailureHandlerFactory, PaymentFailureHandlerFactory>();
        services.AddKeyedScoped<ITransactionHandler, TicketTransactionHandler>(TransactionTypes.Ticket);
        services.AddKeyedScoped<ITransactionHandler, SettlementTransactionHandler>(TransactionTypes.Settlement);
        services.AddKeyedScoped<ITransactionHandler, EscrowConfirmedHandler>(TransactionTypes.Escrow);
        services.AddKeyedScoped<IPaymentFailureHandler, EscrowFailedHandler>(TransactionTypes.Escrow);

        return services;
    }

    public static IServiceCollection AddPaymentDevSeeder(this IServiceCollection services)
    {
        services.AddScoped<IDevSeeder, PaymentDevSeeder>();
        return services;
    }

    public static IServiceCollection AddPaymentTestSeeder(this IServiceCollection services)
    {
        services.AddScoped<ITestSeeder, PaymentTestSeeder>();
        return services;
    }
}
