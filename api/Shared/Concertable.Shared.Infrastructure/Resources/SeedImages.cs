using System.Reflection;

namespace Concertable.Shared.Infrastructure.Resources;

public static class SeedImages
{
    private static readonly Assembly Assembly = typeof(SeedImages).Assembly;

    public static Stream Avatar() =>
        Assembly.GetManifestResourceStream("Concertable.Shared.Infrastructure.Resources.avatar.png")!;

    public static Stream Banner() =>
        Assembly.GetManifestResourceStream("Concertable.Shared.Infrastructure.Resources.banner.png")!;
}
