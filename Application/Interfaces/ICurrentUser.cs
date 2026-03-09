using Application.DTOs;
using Core.Entities;

namespace Application.Interfaces;

public interface ICurrentUser
{
    int? Id { get; }
    int GetId();
    UserDto Get();
    UserDto? GetOrDefault();
    User GetEntity();
    string GetFirstRole();
}
