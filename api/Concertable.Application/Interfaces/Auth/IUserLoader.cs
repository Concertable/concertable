using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Auth;

public interface IUserLoader
{
    Task<UserEntity> LoadAsync(Guid id);
}
