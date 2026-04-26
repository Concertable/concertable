namespace Concertable.Identity.Application.Interfaces;

internal interface IUserMapper
{
    IUser ToDto(UserEntity entity);
    IEnumerable<IUser> ToDtos(IEnumerable<UserEntity> entities) =>
        entities.Select(ToDto);
}
