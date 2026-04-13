using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;

namespace Concertable.Web.E2ETests.Infrastructure;

internal static class DistributedApplicationBuilderExtensions
{
    private const string ApiResourceName = "api";
    private const string ApiBaseUrl = "http://localhost:7001";

    public static IDistributedApplicationTestingBuilder AddE2E(
        this IDistributedApplicationTestingBuilder builder,
        string webhookSecret)
    {
        builder.AddApi(webhookSecret);
        builder.AddEphemeralSql();
        return builder;
    }

    private static void AddApi(
        this IDistributedApplicationTestingBuilder builder,
        string webhookSecret)
    {
        var api = builder.Resources
            .OfType<ProjectResource>()
            .Single(r => r.Name == ApiResourceName);

        api.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
        {
            context.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "E2E";
            context.EnvironmentVariables["ASPNETCORE_URLS"] = ApiBaseUrl;
            context.EnvironmentVariables["Stripe__WebhookSecret"] = webhookSecret;
            context.EnvironmentVariables["ExternalServices__UseRealStripe"] = "true";
            context.EnvironmentVariables["ExternalServices__UseRealEmail"] = "false";
            context.EnvironmentVariables["ExternalServices__UseRealBlob"] = "false";
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
