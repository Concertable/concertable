using Application.DTOs;
using Application.Interfaces;
using Core.Interfaces;
using Core.Parameters;
using GeoCoordinatePortable;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Transactions;

namespace Infrastructure.Services
{
    public class LocationService : ILocationService
    {
        public LocationService() { }

        /// <summary>
        /// Filters items within a specified radius (in km).
        /// </summary>
        public IEnumerable<T> FilterByRadius<T>(double latitude, double longitude, double radiusKm, IEnumerable<T> items)
            where T : ILocation
        {
            return items
                .Where(e => e.Latitude.HasValue && e.Longitude.HasValue)
                .Where(e =>
                {
                    if (e.Latitude == 0 && e.Longitude == 0)
                        return false; 

                    return IsWithinRadius(latitude, longitude, e.Latitude.Value, e.Longitude.Value, (int)radiusKm);
                });
        }

        /// <summary>
        /// Sorts items by distance to a given point.
        /// </summary>
        public IEnumerable<T> SortByDistance<T>(double latitude, double longitude, IEnumerable<T> items, bool ascending = true)
            where T : ILocation
        {
            var inputCoordinate = new GeoCoordinate(latitude, longitude);

            var sortedItems = items.OrderBy(e =>
            {
                if (!e.Latitude.HasValue || !e.Longitude.HasValue)
                    return double.MaxValue; // Push invalid coordinates to the end

                var itemCoordinate = new GeoCoordinate(e.Latitude.Value, e.Longitude.Value);
                return inputCoordinate.GetDistanceTo(itemCoordinate);
            });

            return ascending ? sortedItems : sortedItems.Reverse();
        }

        public IEnumerable<T> FilterAndSortByNearest<T>(double latitude, double longitude, double radiusKm, IEnumerable<T> items, bool ascending = true)
            where T : ILocation
        {
            var filteredItems = FilterByRadius(latitude, longitude, radiusKm, items);

            return SortByDistance(latitude, longitude, filteredItems, ascending);
        }

        public bool IsWithinRadius(double lat1, double lon1,  double lat2, double lon2, int radiusKm)
        {
            var userCoordinate = new GeoCoordinate(lat1, lon1);
            var eventCoordinate = new GeoCoordinate(lat2, lon2);

            double distanceMeters = userCoordinate.GetDistanceTo(eventCoordinate);
            return distanceMeters <= (radiusKm * 1000);
        }
    }
}
