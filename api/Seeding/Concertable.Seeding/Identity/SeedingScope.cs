namespace Concertable.Seeding.Identity;

public sealed class SeedingScope
{
    public bool IsActive { get; private set; }

    public IDisposable Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("SeedingScope is already active.");
        IsActive = true;
        return new Releaser(this);
    }

    private sealed class Releaser : IDisposable
    {
        private readonly SeedingScope scope;
        public Releaser(SeedingScope scope) => this.scope = scope;
        public void Dispose() => scope.IsActive = false;
    }
}
