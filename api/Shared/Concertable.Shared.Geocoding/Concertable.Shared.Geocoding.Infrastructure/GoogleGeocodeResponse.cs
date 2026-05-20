namespace Concertable.Shared.Geocoding.Infrastructure;

internal sealed class GoogleGeocodeResponse
{
    public List<GoogleGeocodeResult> Results { get; set; } = [];
}
