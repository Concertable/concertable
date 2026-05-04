using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.Configuration;

namespace Concertable.E2ETests;

internal static class DistributedApplicationBuilderExtensions
{
    private const string ApiResourceName = "api";
    private const string AuthResourceName = "auth";

    public static StripeCliFixture AddStripe(
        this IDistributedApplicationTestingBuilder builder,
        IConfiguration config,
        string apiBaseUrl)
    {
        var apiKey = config["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey is not configured in appsettings.E2E.json.");

        return new StripeCliFixture(apiKey, apiBaseUrl);
    }

    public static IDistributedApplicationTestingBuilder AddE2E(
        this IDistributedApplicationTestingBuilder builder,
        string apiBaseUrl,
        string authBaseUrl,
        string webhookSecret)
    {
        builder.PinAuth(authBaseUrl);
        builder.PinApi(apiBaseUrl, authBaseUrl, webhookSecret);
        builder.AddEphemeralSql();
        return builder;
    }

    private static void PinApi(
        this IDistributedApplicationTestingBuilder builder,
        string apiBaseUrl,
        string authBaseUrl,
        string webhookSecret)
    {
        var api = builder.Resources
            .OfType<ProjectResource>()
            .Single(r => r.Name == ApiResourceName);

        api.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
        {
            context.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "E2E";
            context.EnvironmentVariables["ASPNETCORE_URLS"] = apiBaseUrl;
            context.EnvironmentVariables["Auth__Authority"] = authBaseUrl;
            context.EnvironmentVariables["Stripe__WebhookSecret"] = webhookSecret;
            context.EnvironmentVariables["ExternalServices__UseRealStripe"] = "true";
            context.EnvironmentVariables["ExternalServices__UseRealEmail"] = "false";
            context.EnvironmentVariables["ExternalServices__UseRealBlob"] = "false";
        }));
    }

    private static void PinAuth(
        this IDistributedApplicationTestingBuilder builder,
        string authBaseUrl)
    {
        var auth = builder.Resources
            .OfType<ProjectResource>()
            .Single(r => r.Name == AuthResourceName);

        auth.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
        {
            context.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "E2E";
            context.EnvironmentVariables["ASPNETCORE_URLS"] = authBaseUrl;
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
