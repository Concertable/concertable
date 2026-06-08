namespace Concertable.Shared.Geocoding.Infrastructure;

internal sealed class GoogleAddressComponent
{
    public string Long_Name { get; set; } = string.Empty;
    public List<string> Types { get; set; } = [];
}
