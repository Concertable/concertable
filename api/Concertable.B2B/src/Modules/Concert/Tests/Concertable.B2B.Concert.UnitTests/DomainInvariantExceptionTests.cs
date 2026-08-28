using System.Runtime.CompilerServices;
using Xunit;

namespace Concertable.B2B.Concert.UnitTests;

/// <summary>
/// Guards the convention (`api/Concertable.B2B/ARCHITECTURE.md`): a broken domain invariant throws
/// <c>Concertable.Kernel.DomainException</c>, never <c>InvalidOperationException</c>. The latter is drift
/// this test exists to stop recurring.
/// </summary>
public sealed class DomainInvariantExceptionTests
{
    [Fact]
    public void Concert_domain_never_throws_InvalidOperationException()
    {
        var domainRoot = DomainProjectRoot();

        var offenders = Directory
            .EnumerateFiles(domainRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                        && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .Where(path => File.ReadAllText(path).Contains("throw new InvalidOperationException"))
            .Select(path => Path.GetFileName(path))
            .OrderBy(name => name)
            .ToArray();

        Assert.Empty(offenders);
    }

    private static string DomainProjectRoot([CallerFilePath] string thisFile = "")
    {
        // …/src/Modules/Concert/Tests/Concertable.B2B.Concert.UnitTests/DomainInvariantExceptionTests.cs
        var testProjectDir = Path.GetDirectoryName(thisFile)!;
        return Path.GetFullPath(Path.Combine(testProjectDir, "..", "..", "Concertable.B2B.Concert.Domain"));
    }
}
