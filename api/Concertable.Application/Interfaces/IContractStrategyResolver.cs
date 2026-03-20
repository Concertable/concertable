using Application.Interfaces.Concert;

namespace Application.Interfaces;

public interface IContractStrategyResolver<T> where T : IContractStrategy
{
    Task<T> ResolveForConcertAsync(int concertId);
    Task<T> ResolveForApplicationAsync(int applicationId);
}
