using Application.DTOs;
using Core.Entities;
using Core.Enums;

namespace Application.Interfaces;

public interface ICurrentUser
{
    Guid? Id { get; }
    Guid GetId();
    UserDto Get();
    UserDto? GetOrDefault();
    UserEntity GetEntity();
    Role GetRole();
}
