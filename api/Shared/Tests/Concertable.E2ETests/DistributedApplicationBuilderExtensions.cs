using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.Configuration;

namespace Concertable.E2ETests;

internal static class DistributedApplicationBuilderExtensions
{
    private const string StripeCliResourceName = AppHostConstants.ResourceNames.StripeCli;

    public static IDistributedApplicationTestingBuilder AddE2E(
        this IDistributedApplicationTestingBuilder builder,
        string apiBaseUrl,
        string customerApiBaseUrl,
        string searchApiBaseUrl,
        string authBaseUrl,
        string paymentBaseUrl)
    {
        builder.PinAuthService(authBaseUrl);
        builder.PinB2BWeb(apiBaseUrl, authBaseUrl, paymentBaseUrl);
        builder.PinCustomerWeb(customerApiBaseUrl, authBaseUrl, paymentBaseUrl);
        builder.PinSearchWeb(searchApiBaseUrl, authBaseUrl);
        builder.PinPaymentWeb(paymentBaseUrl, authBaseUrl);
        builder.PinPaymentWorkers();
        builder.AddEphemeralSql();
        builder.PinStripeCli(paymentBaseUrl);
        return builder;
    }

    public static IDistributedApplicationTestingBuilder AddB2BE2E(
        this IDistributedApplicationTestingBuilder builder,
        string apiBaseUrl,
        string searchApiBaseUrl,
        string authBaseUrl,
        string paymentBaseUrl)
    {
        builder.PinAuthService(authBaseUrl);
        builder.PinB2BWeb(apiBaseUrl, authBaseUrl, paymentBaseUrl);
        builder.PinSearchWeb(searchApiBaseUrl, authBaseUrl);
        builder.PinPaymentWeb(paymentBaseUrl, authBaseUrl);
        builder.PinPaymentWorkers();
        builder.AddEphemeralSql();
        builder.PinStripeCli(paymentBaseUrl);
        return builder;
    }

    public static IDistributedApplicationTestingBuilder AddCustomerE2E(
        this IDistributedApplicationTestingBuilder builder,
        string customerApiBaseUrl,
        string searchApiBaseUrl,
        string authBaseUrl,
        string paymentBaseUrl)
    {
        builder.PinAuthService(authBaseUrl);
        builder.PinCustomerWeb(customerApiBaseUrl, authBaseUrl, paymentBaseUrl);
        builder.PinSearchWeb(searchApiBaseUrl, authBaseUrl);
        builder.PinPaymentWeb(paymentBaseUrl, authBaseUrl);
        builder.PinPaymentWorkers();
        builder.AddEphemeralSql();
        builder.PinStripeCli(paymentBaseUrl);
        return builder;
    }

    private static void PinB2BWeb(
        this IDistributedApplicationTestingBuilder builder,
        string apiBaseUrl,
        string authBaseUrl,
        string paymentBaseUrl)
    {
        var b2bWeb = builder.Resources
            .OfType<ProjectResource>()
            .Single(r => r.Name == AppHostConstants.ResourceNames.B2BWeb);

        var googleApiKey = builder.Configuration["GoogleApiKey"];
        var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];

        b2bWeb.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
        {
            context.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "E2E";
            context.EnvironmentVariables["ASPNETCORE_URLS"] = apiBaseUrl;
            context.EnvironmentVariables["Auth__Authority"] = authBaseUrl;
            context.EnvironmentVariables["services__payment-web__https__0"] = paymentBaseUrl;
            context.EnvironmentVariables["ExternalServices__UseRealStripe"] = "true";
            context.EnvironmentVariables["ExternalServices__UseRealEmail"] = "false";
            if (!string.IsNullOrEmpty(googleApiKey))
                context.EnvironmentVariables["GoogleApiKey"] = googleApiKey;
            if (!string.IsNullOrEmpty(stripeSecretKey))
                context.EnvironmentVariables["Stripe__SecretKey"] = stripeSecretKey;
        }));
    }

    private static void PinCustomerWeb(
        this IDistributedApplicationTestingBuilder builder,
        string customerApiBaseUrl,
        string authBaseUrl,
        string paymentBaseUrl)
    {
        var customerWeb = builder.Resources
            .OfType<ProjectResource>()
            .Single(r => r.Name == AppHostConstants.ResourceNames.CustomerWeb);

        customerWeb.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
        {
            context.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "E2E";
            context.EnvironmentVariables["ASPNETCORE_URLS"] = customerApiBaseUrl;
            context.EnvironmentVariables["Auth__Authority"] = authBaseUrl;
            context.EnvironmentVariables["services__payment-web__https__0"] = paymentBaseUrl;
        }));
    }

    private static void PinSearchWeb(
        this IDistributedApplicationTestingBuilder builder,
        string searchApiBaseUrl,
        string authBaseUrl)
    {
        var searchWeb = builder.Resources
            .OfType<ProjectResource>()
            .Single(r => r.Name == AppHostConstants.ResourceNames.SearchWeb);

        searchWeb.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
        {
            context.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "E2E";
            context.EnvironmentVariables["ASPNETCORE_URLS"] = searchApiBaseUrl;
            context.EnvironmentVariables["Auth__Authority"] = authBaseUrl;
        }));
    }

    private static void PinPaymentWeb(
        this IDistributedApplicationTestingBuilder builder,
        string paymentBaseUrl,
        string authBaseUrl)
    {
        var paymentWeb = builder.Resources
            .OfType<ProjectResource>()
            .Single(r => r.Name == AppHostConstants.ResourceNames.PaymentWeb);

        var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];

        paymentWeb.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
        {
            context.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "E2E";
            context.EnvironmentVariables["ASPNETCORE_URLS"] = paymentBaseUrl;
            context.EnvironmentVariables["Auth__Authority"] = authBaseUrl;
            context.EnvironmentVariables["Stripe__SkipWebhookVerification"] = "true";
            if (!string.IsNullOrEmpty(stripeSecretKey))
                context.EnvironmentVariables["Stripe__SecretKey"] = stripeSecretKey;
        }));
    }

    private static void PinAuthService(
        this IDistributedApplicationTestingBuilder builder,
        string authBaseUrl)
    {
        var auth = builder.Resources
            .OfType<ProjectResource>()
            .Single(r => r.Name == AppHostConstants.ResourceNames.Auth);

        auth.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
        {
            context.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "E2E";
            context.EnvironmentVariables["ASPNETCORE_URLS"] = authBaseUrl;
        }));
    }

    private static void PinPaymentWorkers(this IDistributedApplicationTestingBuilder builder)
    {
        var paymentWorkers = builder.Resources
            .OfType<ProjectResource>()
            .Single(r => r.Name == AppHostConstants.ResourceNames.PaymentWorkers);

        paymentWorkers.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
        {
            context.EnvironmentVariables["DOTNET_ENVIRONMENT"] = "E2E";
        }));
    }

    private static void PinStripeCli(
        this IDistributedApplicationTestingBuilder builder,
        string paymentBaseUrl)
    {
        var stripeCli = builder.Resources
            .OfType<ContainerResource>()
            .FirstOrDefault(r => r.Name == AppHostConstants.ResourceNames.StripeCli);

        if (stripeCli is null) return;

        var apiKey = builder.Configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey is not configured.");
        var forwardTo = $"{paymentBaseUrl.Replace("localhost", "host.docker.internal")}/api/Webhook";

        foreach (var annotation in stripeCli.Annotations.OfType<CommandLineArgsCallbackAnnotation>().ToList())
            stripeCli.Annotations.Remove(annotation);

        var volume = stripeCli.Annotations.OfType<ContainerMountAnnotation>()
            .FirstOrDefault(m => m.Source == "stripe-cli-config");
        if (volume is not null)
            stripeCli.Annotations.Remove(volume);

        stripeCli.Annotations.Add(new CommandLineArgsCallbackAnnotation(ctx =>
        {
            ctx.Args.Add("listen");
            ctx.Args.Add("--skip-verify");
            ctx.Args.Add("--api-key");
            ctx.Args.Add(apiKey);
            ctx.Args.Add("--forward-to");
            ctx.Args.Add(forwardTo);
            return Task.CompletedTask;
        }));
    }

    private static void AddEphemeralSql(
        this IDistributedApplicationTestingBuilder builder)
    {
        var sql = builder.Resources
            .OfType<SqlServerServerResource>()
            .Single();

        var volume = sql.Annotations
            .OfType<ContainerMountAnnotation>()
            .FirstOrDefault();

        if (volume is not null)
            sql.Annotations.Remove(volume);
    }
}
