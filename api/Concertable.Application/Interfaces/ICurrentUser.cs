using Concertable.Application.DTOs;
using Concertable.Core.Entities;
using Concertable.Core.Enums;

namespace Concertable.Application.Interfaces;

public interface ICurrentUser
{
    Guid? Id { get; }
    Guid GetId();
    UserDto Get();
    UserDto? GetOrDefault();
    UserEntity GetEntity();
    Role GetRole();
}
