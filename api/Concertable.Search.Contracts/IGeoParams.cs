namespace Concertable.Search.Contracts;

public interface IGeoParams
{
    double? Latitude { get; }
    double? Longitude { get; }
    int? RadiusKm { get; }
}
