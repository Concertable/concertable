namespace Concertable.Tests.Common;

public sealed class FakeTimeProvider : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(2026, 5, 4, 12, 0, 0, TimeSpan.Zero);
}
