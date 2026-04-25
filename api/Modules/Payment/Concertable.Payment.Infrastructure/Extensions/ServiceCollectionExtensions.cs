using Concertable.Application.Interfaces;
using Concertable.Data.Infrastructure;
using Concertable.Data.Infrastructure.Data;
using Concertable.Payment.Infrastructure.Background;
using Concertable.Payment.Infrastructure.Data;
using Concertable.Payment.Infrastructure.Data.Seeders;
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
            services.AddScoped<IStripeAccountService, StripeAccountService>();
            services.AddSingleton<IStripePaymentClient, StripePaymentClient>();
            services.AddKeyedScoped<IPaymentService, OnSessionPaymentService>("onSession");
            services.AddKeyedScoped<IPaymentService, OffSessionPaymentService>("offSession");
            services.AddScoped<IWebhookService, WebhookService>();
        }
        else
        {
            services.AddScoped<IStripeAccountService, FakeStripeAccountService>();
            services.AddKeyedScoped<IPaymentService, FakePaymentService>("onSession");
            services.AddKeyedScoped<IPaymentService, FakePaymentService>("offSession");
            services.AddScoped<IWebhookService, FakeWebhookService>();
        }

        // Stripe validation (keyed by ContractType)
        services.AddScoped<StripeAccountValidator>();
        services.AddScoped<StripeCustomerValidator>();
        services.AddScoped<IStripeValidator, StripeValidator>();
        services.AddScoped<IStripeValidationFactory, StripeValidationFactory>();
        services.AddKeyedScoped<IStripeValidationStrategy, StripeAccountValidator>(ContractType.VenueHire);
        services.AddKeyedScoped<IStripeValidationStrategy, StripeCustomerValidator>(ContractType.FlatFee);
        services.AddKeyedScoped<IStripeValidationStrategy, StripeCustomerValidator>(ContractType.DoorSplit);
        services.AddKeyedScoped<IStripeValidationStrategy, StripeCustomerValidator>(ContractType.Versus);

        // Payment services
        services.AddScoped<ICustomerPaymentService, CustomerPaymentService>();
        AddKeyedManagerPaymentService(services, "onSession");
        AddKeyedManagerPaymentService(services, "offSession");

        // Ticket payment strategies (keyed by ContractType)
        services.AddKeyedScoped<ITicketPaymentStrategy, VenueTicketPaymentService>(ContractType.FlatFee);
        services.AddKeyedScoped<ITicketPaymentStrategy, VenueTicketPaymentService>(ContractType.DoorSplit);
        services.AddKeyedScoped<ITicketPaymentStrategy, VenueTicketPaymentService>(ContractType.Versus);
        services.AddKeyedScoped<ITicketPaymentStrategy, ArtistTicketPaymentService>(ContractType.VenueHire);
        services.AddScoped<ITicketPaymentStrategyFactory, TicketPaymentStrategyFactory>();

        // Webhook infrastructure
        services.AddScoped<IWebhookStrategyFactory, WebhookStrategyFactory>();
        services.AddScoped<IWebhookProcessor, WebhookProcessor>();
        services.AddScoped<IWebhookQueue, WebhookQueue>();
        services.AddKeyedScoped<IWebhookStrategy, TicketWebhookHandler>(WebhookType.Concert);
        services.AddKeyedScoped<IWebhookStrategy, SettlementWebhookHandler>(WebhookType.Settlement);

        // Background queue (Singleton; QueueHostedService registered in Web host via AddPaymentQueueHostedService)
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddSingleton<IBackgroundTaskRunner, BackgroundTaskRunner>();

        // Module facade
        services.AddScoped<IPaymentModule, PaymentModule>();

        return services;
    }

    public static IServiceCollection AddPaymentQueueHostedService(this IServiceCollection services)
    {
        services.AddHostedService<QueueHostedService>();
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

    private static void AddKeyedManagerPaymentService(IServiceCollection services, string key) =>
        services.AddKeyedScoped<IManagerPaymentService>(key, (sp, _) =>
            new ManagerPaymentService(
                sp.GetRequiredService<IStripeAccountService>(),
                sp.GetRequiredKeyedService<IPaymentService>(key),
                sp.GetRequiredService<ITransactionService>(),
                sp.GetRequiredService<IPayoutAccountRepository>(),
                sp.GetRequiredService<TimeProvider>()));
}
