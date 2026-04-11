namespace Concertable.Application.Interfaces;

public interface IPagination<out T>
{
    IEnumerable<T> Data { get; }
    int TotalCount { get; }
    int TotalPages { get; }
    int PageNumber { get; }
    int PageSize { get; }
}
