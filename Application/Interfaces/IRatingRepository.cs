namespace Application.Interfaces;

public interface IRatingRepository
{
    Task<IDictionary<int, double>> GetRatingsAsync(IEnumerable<int> ids);
}
