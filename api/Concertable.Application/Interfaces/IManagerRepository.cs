using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces;

public interface IManagerRepository<T> : IGuidRepository<T> where T : ManagerEntity
{
    Task<T?> GetByApplicationIdAsync(int applicationId);
    Task<T?> GetByConcertIdAsync(int concertId);
}
