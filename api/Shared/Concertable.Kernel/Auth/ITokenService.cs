namespace Concertable.Shared.Infrastructure.Services;

public interface ITokenService
{
    Task<string> GetTokenAsync(string scope, CancellationToken ct = default);
}
