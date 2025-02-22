using Application.DTOs;
using Application.Interfaces;
using Core.Parameters;
using GeoCoordinatePortable; // Import the GeoCoordinate package
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Services
{
    public class LocationService : ILocationService
    {
        public LocationService() { }

        public IEnumerable<T> FilterAndSortByNearest<T>(SearchParams searchParams, IEnumerable<T> headersDto)
            where T : HeaderDto
        {
            if (searchParams == null || !searchParams.Latitude.HasValue || !searchParams.Longitude.HasValue)
                return headersDto; 

            var inputCoordinate = new GeoCoordinate(searchParams.Latitude.Value, searchParams.Longitude.Value);
            var radiusKm = searchParams.RadiusKm ?? 10; 

            var filteredResults = headersDto
                .Where(e => e.Latitude.HasValue && e.Longitude.HasValue) 
                .Where(e =>
                {
                    if (e.Latitude == 0 && e.Longitude == 0)
                        return false; //i.e. Dont filter it

                    var itemCoordinate = new GeoCoordinate(e.Latitude.Value, e.Longitude.Value);
                    return inputCoordinate.GetDistanceTo(itemCoordinate) <= radiusKm * 1000; // Compare with radius in meters
                });

            if (!string.IsNullOrEmpty(searchParams.Sort))
            {
                if (string.Equals(searchParams.Sort, "location_asc", StringComparison.OrdinalIgnoreCase))
                {
                    filteredResults = filteredResults.OrderBy(e =>
                    {
                        if (e.Latitude.HasValue && e.Longitude.HasValue)
                        {
                            var itemCoordinate = new GeoCoordinate(e.Latitude.Value, e.Longitude.Value);
                            return inputCoordinate.GetDistanceTo(itemCoordinate); 
                        }
                        return double.MaxValue; // Push invalid coordinates to the end
                    });
                }
                else if (string.Equals(searchParams.Sort, "location_desc", StringComparison.OrdinalIgnoreCase))
                {
                    filteredResults = filteredResults.OrderByDescending(e =>
                    {
                        if (e.Latitude.HasValue && e.Longitude.HasValue)
                        {
                            var itemCoordinate = new GeoCoordinate(e.Latitude.Value, e.Longitude.Value);
                            return inputCoordinate.GetDistanceTo(itemCoordinate); 
                        }
                        return double.MinValue; // Push invalid coordinates to the end
                    });
                }
            }
            return filteredResults; 
        }
    }
}
