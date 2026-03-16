using Core.Enums;

namespace Application.Interfaces.Search;

public interface IHeaderServiceFactory
{
    IHeaderService? Create(HeaderType type);
}
