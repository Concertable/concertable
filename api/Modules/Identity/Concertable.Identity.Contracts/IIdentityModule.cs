namespace Concertable.Identity.Contracts;

public interface IIdentityModule
{
    Task<ManagerDto?> GetManagerAsync(Guid userId);
}
