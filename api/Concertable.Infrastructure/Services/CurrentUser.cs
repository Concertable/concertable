using Concertable.Application.Interfaces;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Application.Exceptions;

namespace Concertable.Infrastructure.Services;

public class CurrentUser
{
    private readonly IUser dto;
    private readonly UserEntity entity;

    public CurrentUser(UserEntity entity, IUserMapper mapper)
    {
        this.entity = entity;
        this.dto = mapper.ToDto(entity);
    }

    public Guid Id => dto.Id;

    public IUser Get() => dto;

    public IUser? GetOrDefault() => dto;

    public UserEntity GetEntity() => entity;

    public T GetEntity<T>() where T : UserEntity =>
        entity as T ?? throw new UnauthorizedException("Unauthorized");

    public Role GetRole() =>
        dto.Role ?? throw new BadRequestException("User has no roles assigned.");
}
