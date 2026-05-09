using Docker.DotNet;
using Docker.DotNet.Models;

namespace Concertable.E2ETests;

internal static class DockerLeakReaper
{
    private static readonly IDictionary<string, IDictionary<string, bool>> LabelFilters =
        new Dictionary<string, IDictionary<string, bool>>
        {
            ["label"] = new Dictionary<string, bool>
            {
                ["com.microsoft.developer.usvc-dev.name"] = true,
                ["concertable-e2e=1"] = true,
            },
        };

    public static async Task ReapAsync(CancellationToken ct = default)
    {
        using var client = new DockerClientConfiguration().CreateClient();

        var containers = await client.Containers.ListContainersAsync(
            new ContainersListParameters { All = true, Filters = LabelFilters },
            ct);

        await Task.WhenAll(containers.Select(c =>
            client.Containers.RemoveContainerAsync(
                c.ID,
                new ContainerRemoveParameters { Force = true },
                ct)));
    }
}
