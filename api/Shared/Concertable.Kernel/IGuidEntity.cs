namespace Concertable.Kernel;

public interface IGuidEntity : IEntity
{
    Guid Id { get; }
}
