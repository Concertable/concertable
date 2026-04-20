
namespace Concertable.Identity.Application.Interfaces;

public interface IUserMapper
{
    IUser ToDto(UserEntity entity);
}
