namespace Application.Responses;

public interface IPagination<out T>
{
    IEnumerable<T> Data { get; }
    int TotalCount { get; }
    int PageNumber { get; }
    int PageSize { get; }
}
