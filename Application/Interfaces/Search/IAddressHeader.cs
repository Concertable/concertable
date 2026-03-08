namespace Application.Interfaces.Search;

public interface IAddressHeader
{
    string County { get; set; }
    string Town { get; set; }
    double? Latitude { get; set; }
    double? Longitude { get; set; }
}
