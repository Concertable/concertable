namespace Concertable.Identity.Contracts;

public interface IIdentityModule
{
    Task<IUser?> GetUserByIdAsync(Guid id);
}
