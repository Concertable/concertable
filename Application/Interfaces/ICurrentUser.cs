using Application.DTOs;
using Core.Entities.Identity;

namespace Application.Interfaces;

public interface ICurrentUser
{
    int? Id { get; }
    int GetId();
    UserDto Get();
    UserDto? GetOrDefault();
    ApplicationUser GetEntity();
    string GetFirstRole();
}
