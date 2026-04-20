
namespace Concertable.Identity.Application.Interfaces;

internal interface IManagerRepository<T> : IGuidRepository<T> where T : ManagerEntity
{
    Task<T?> GetByApplicationIdAsync(int applicationId);
    Task<T?> GetByConcertIdAsync(int concertId);
}
