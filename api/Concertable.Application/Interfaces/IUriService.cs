namespace Concertable.Application.Interfaces;

public interface IUriService
{
    Uri GetUri(string path, IDictionary<string, string>? query = null);
}
