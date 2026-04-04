using Concertable.Core.Enums;

namespace Concertable.Application.Interfaces.Search;

public interface IHeaderServiceFactory
{
    IHeaderService Create(HeaderType type);
}
