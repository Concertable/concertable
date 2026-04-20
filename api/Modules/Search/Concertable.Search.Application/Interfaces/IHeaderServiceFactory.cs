using Concertable.Core.Enums;

namespace Concertable.Search.Application.Interfaces;

internal interface IHeaderServiceFactory
{
    IHeaderService Create(HeaderType type);
}
