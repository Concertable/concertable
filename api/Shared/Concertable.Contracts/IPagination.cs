namespace Concertable.Shared;

public interface IPagination<out T>
{
    IReadOnlyList<T> Data { get; }
    int TotalCount { get; }
    int TotalPages { get; }
    int PageNumber { get; }
    int PageSize { get; }
}
