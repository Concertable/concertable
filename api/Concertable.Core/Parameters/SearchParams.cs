using Concertable.Core.Enums;
using Concertable.Core.Interfaces;

namespace Concertable.Core.Parameters;

public class SearchParams : IPageParams, IGeoParams
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public HeaderType? HeaderType { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public string? Sort { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? RadiusKm { get; set; }
    public int[]? GenreIds { get; set; }
    public bool? ShowHistory { get; set; }
    public bool? ShowSold { get; set; }
}
