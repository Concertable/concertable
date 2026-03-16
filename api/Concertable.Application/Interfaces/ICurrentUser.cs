using Application.DTOs;
using Core.Entities;
using Core.Enums;

namespace Application.Interfaces;

public interface ICurrentUser
{
    int? Id { get; }
    int GetId();
    UserDto Get();
    UserDto? GetOrDefault();
    UserEntity GetEntity();
    Role GetRole();
}
