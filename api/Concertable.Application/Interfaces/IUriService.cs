namespace Concertable.Application.Interfaces;

public interface IUriService
{
    Uri GetUri(string path, Dictionary<string, string>? query = null);
}
