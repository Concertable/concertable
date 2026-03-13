using Application.DTOs;
using Core.Entities;

namespace Application.Interfaces;

public interface IUserService
{
    Task<User> GetByApplicationIdAsync(int applicationId);
    Task<User> GetByConcertIdAsync(int id);
    Task<int> GetIdByApplicationIdAsync(int id);
    Task<int> GetIdByConcertIdAsync(int id);
    Task<UserDto> UpdateLocationAsync(double latitude, double longitude);
}
