namespace Concertable.Application.Interfaces.Rating;

public interface IRatingEnricher
{
    Task EnrichAsync(IEnumerable<IHasRating> headers);
}
