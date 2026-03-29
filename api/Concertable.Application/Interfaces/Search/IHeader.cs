using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Rating;

namespace Concertable.Application.Interfaces.Search;

public interface IHeader : IHasRating, IAddress, ILatLong
{
    string Name { get; set; }
    string ImageUrl { get; set; }
}
