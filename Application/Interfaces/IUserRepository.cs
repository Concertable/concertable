using Core.Entities;

namespace Application.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<int> GetIdByApplicationIdAsync(int applicationId);
    Task<int> GetIdByConcertIdAsync(int concertId);
    Task<User> GetByApplicationIdAsync(int applicationId);
    Task<User> GetByConcertIdAsync(int concertId);
    Task<bool> ExistsByEmailAsync(string email);
}
