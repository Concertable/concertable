using Core.Entities;

namespace Application.Interfaces;

public interface IUserRepository : IBaseRepository<UserEntity>
{
    Task<int> GetIdByApplicationIdAsync(int applicationId);
    Task<int> GetIdByConcertIdAsync(int concertId);
    Task<UserEntity> GetByApplicationIdAsync(int applicationId);
    Task<UserEntity> GetByConcertIdAsync(int concertId);
    Task<bool> ExistsByEmailAsync(string email);
    Task<UserEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
