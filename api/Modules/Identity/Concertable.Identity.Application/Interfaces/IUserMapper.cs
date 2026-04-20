
namespace Concertable.Identity.Application.Interfaces;

internal interface IUserMapper
{
    IUser ToDto(UserEntity entity);
}
