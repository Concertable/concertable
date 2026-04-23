namespace Concertable.Identity.Contracts;

public interface IManagerModule
{
    Task<ManagerDto?> GetByIdAsync(Guid userId);
}
