using Concertable.Core.Enums;

namespace Concertable.Search.Application.Interfaces;

public interface IHeaderServiceFactory
{
    IHeaderService Create(HeaderType type);
}
