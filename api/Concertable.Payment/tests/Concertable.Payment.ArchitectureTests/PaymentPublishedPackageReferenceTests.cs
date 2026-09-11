using System.Xml.Linq;
using Xunit;

namespace Concertable.Payment.ArchitectureTests;

public sealed class PaymentPublishedPackageReferenceTests
{
    private static readonly string[] AllowedConcertableDependencyPrefixes =
    [
        "Concertable.Contracts",
        "Concertable.DataAccess.",
        "Concertable.Grpc",
        "Concertable.Kernel",
        "Concertable.Messaging.",
        "Concertable.Payment.",
        "Concertable.Seed.Shared",
        "Concertable.ServiceDefaults",
        "Concertable.Shared.Api"
    ];

    // Host-only because this reference would otherwise follow Concertable.Payment.Client into every
    // consumer of Payment and couple them to Auth's release train.
    private static readonly string[] AllowedHostOnlyDependencyPrefixes =
        [.. AllowedConcertableDependencyPrefixes, "Concertable.Auth.Contracts"];

    private static readonly string[] CompositionProjectSuffixes = [".AppHost", ".Hosting"];

    private static readonly string[] HostProjectSuffixes = [".Web"];

    [Fact]
    public void PaymentDeployableProjects_ReferenceOnlySharedOrPaymentAssemblies()
    {
        var sourceRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src"));
        var unexpected = Directory.EnumerateFiles(sourceRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !HasSuffix(path, CompositionProjectSuffixes))
            .SelectMany(path => XDocument.Load(path).Descendants()
                .Where(element => element.Name.LocalName is "ProjectReference" or "PackageReference")
                .Select(element => new
                {
                    Project = Path.GetRelativePath(sourceRoot, path),
                    Allowed = HasSuffix(path, HostProjectSuffixes)
                        ? AllowedHostOnlyDependencyPrefixes
                        : AllowedConcertableDependencyPrefixes,
                    Reference = (string?)element.Attribute("Include")
                }))
            .Where(item => item.Reference?.StartsWith("Concertable.", StringComparison.Ordinal) == true
                && !item.Allowed.Any(prefix => item.Reference.StartsWith(prefix, StringComparison.Ordinal)))
            .ToArray();

        Assert.Empty(unexpected);
    }

    private static bool HasSuffix(string projectPath, string[] suffixes) =>
        suffixes.Any(suffix => Path.GetFileNameWithoutExtension(projectPath).EndsWith(suffix, StringComparison.Ordinal));
}
