
namespace Concertable.Application.Interfaces.Auth;

public interface IUserLoader
{
    Task<UserEntity> LoadAsync(UserEntity user);
}
