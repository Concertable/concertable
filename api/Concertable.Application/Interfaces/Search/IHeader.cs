using Application.Interfaces;
using Application.Interfaces.Rating;

namespace Application.Interfaces.Search;

public interface IHeader : IHasRating, IAddress, ILatLong
{
    string Name { get; set; }
    string ImageUrl { get; set; }
}
