namespace Concertable.Shared.Geocoding.Infrastructure;

internal sealed class GoogleGeocodeResult
{
    public List<GoogleAddressComponent> Address_Components { get; set; } = [];
}
