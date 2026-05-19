namespace Concertable.DataAccess;

public interface IUriService
{
    Uri GetUri(string path, IDictionary<string, string>? query = null);
}
