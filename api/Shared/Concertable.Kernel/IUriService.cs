namespace Concertable.Shared.Infrastructure.Services;

public interface IUriService
{
    Uri GetUri(string path, IDictionary<string, string>? query = null);
}
