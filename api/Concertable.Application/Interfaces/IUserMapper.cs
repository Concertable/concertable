using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces;

public interface IUserMapper
{
    IUser ToDto(UserEntity entity);
}
