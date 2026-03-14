namespace Application.Interfaces;

public interface IRatingRepository
{
    Task<double> GetRatingAsync(int id);
    Task<IDictionary<int, double>> GetRatingsAsync(IEnumerable<int> ids);
}
