namespace Application.Interfaces;

public interface IRatingEnricher
{
    Task EnrichAsync(IEnumerable<IHasRating> headers);
}
