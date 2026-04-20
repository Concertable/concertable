
namespace Concertable.Identity.Application.Interfaces.Auth;

internal interface IUserLoader
{
    Task<UserEntity> LoadAsync(UserEntity user);
}
