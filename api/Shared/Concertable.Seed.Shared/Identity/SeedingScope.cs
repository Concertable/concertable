namespace Concertable.Seed.Identity;

public sealed class SeedingScope : IDisposable
{
    public bool IsActive { get; private set; }

    public IDisposable Activate()
    {
        IsActive = true;
        return this;
    }

    public void Dispose() => IsActive = false;
}
