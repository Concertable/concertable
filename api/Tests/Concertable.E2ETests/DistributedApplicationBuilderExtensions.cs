using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.Configuration;

namespace Concertable.E2ETests;

internal static class DistributedApplicationBuilderExtensions
{
    private const string ApiResourceName = "api";
    private const string AuthResourceName = "auth";
    private const string StripeCliResourceName = "stripe-cli";

    public static IDistributedApplicationTestingBuilder AddE2E(
        this IDistributedApplicationTestingBuilder builder,
        string apiBaseUrl,
        string authBaseUrl)
    {
        builder.PinAuth(authBaseUrl);
        builder.PinApi(apiBaseUrl, authBaseUrl);
        builder.AddEphemeralSql();
        builder.PinStripeCli(apiBaseUrl);
        return builder;
    }

    private static void PinApi(
        this IDistributedApplicationTestingBuilder builder,
        string apiBaseUrl,
        string authBaseUrl)
    {
        var api = builder.Resources
            .OfType<ProjectResource>()
            .Single(r => r.Name == ApiResourceName);

        var googleApiKey = builder.Configuration["GoogleApiKey"];
        var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];

        api.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
        {
            context.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "E2E";
            context.EnvironmentVariables["ASPNETCORE_URLS"] = apiBaseUrl;
            context.EnvironmentVariables["Auth__Authority"] = authBaseUrl;
            context.EnvironmentVariables["ExternalServices__UseRealStripe"] = "true";
            context.EnvironmentVariables["ExternalServices__UseRealEmail"] = "false";
            context.EnvironmentVariables["ExternalServices__UseRealBlob"] = "false";
            if (!string.IsNullOrEmpty(googleApiKey))
                context.EnvironmentVariables["GoogleApiKey"] = googleApiKey;
            if (!string.IsNullOrEmpty(stripeSecretKey))
                context.EnvironmentVariables["Stripe__SecretKey"] = stripeSecretKey;
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

    private static void PinStripeCli(
        this IDistributedApplicationTestingBuilder builder,
        string apiBaseUrl)
    {
        var stripeCli = builder.Resources
            .OfType<ContainerResource>()
            .FirstOrDefault(r => r.Name == StripeCliResourceName);

        if (stripeCli is null) return;

        var apiKey = builder.Configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey is not configured.");
        var forwardTo = $"{apiBaseUrl.Replace("localhost", "host.docker.internal")}/api/Webhook";

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
