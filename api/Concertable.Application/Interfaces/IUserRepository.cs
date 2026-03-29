using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces;

public interface IUserRepository : IGuidRepository<UserEntity>
{
    Task<Guid> GetIdByApplicationIdAsync(int applicationId);
    Task<Guid> GetIdByConcertIdAsync(int concertId);
    Task<UserEntity> GetByApplicationIdAsync(int applicationId);
    Task<UserEntity> GetByConcertIdAsync(int concertId);
    Task<bool> ExistsByEmailAsync(string email);
    Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
