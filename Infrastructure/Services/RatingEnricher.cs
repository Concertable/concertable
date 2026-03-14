using Application.Interfaces;

namespace Infrastructure.Services;

public class RatingEnricher : IRatingEnricher
{
    private readonly IRatingRepository ratingRepository;

    public RatingEnricher(IRatingRepository ratingRepository)
    {
        this.ratingRepository = ratingRepository;
    }

    public async Task EnrichAsync(IEnumerable<IHasRating> headers)
    {
        var headerList = headers.ToList();
        if (headerList.Count == 0)
            return;

        var ids = headerList.Select(h => h.Id).ToList();
        var ratings = await ratingRepository.GetRatingsAsync(ids);

        foreach (var h in headerList)
            h.Rating = ratings.TryGetValue(h.Id, out var rating) ? rating : 0.0;
    }
}
