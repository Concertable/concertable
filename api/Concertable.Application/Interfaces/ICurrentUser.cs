using Concertable.Core.Entities;
using Concertable.Core.Enums;

namespace Concertable.Application.Interfaces;

public interface ICurrentUser
{
    Guid? Id { get; }
    Guid GetId();
    IUser Get();
    IUser? GetOrDefault();
    UserEntity GetEntity();
    T GetEntity<T>() where T : UserEntity;
    Role GetRole();
}
