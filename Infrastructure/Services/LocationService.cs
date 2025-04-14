using Application.DTOs;
using Application.Interfaces;
using Common.Helpers;
using Core.Interfaces;
using Core.Parameters;
using GeoCoordinatePortable;
using MimeKit;
using NetTopologySuite.Geometries;
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
        public IQueryable<T> FilterByRadius<T>(
            IQueryable<T> query,
            double latitude,
            double longitude,
            double radiusKm)
            where T : ILocation
        {
            //var userLocation = new Point(longitude, latitude)
            //{
            //    SRID = 4326  // Standard for lat/lon (WGS 84)
            //};

            //var radiusMeters = radiusKm * 1000;  // Convert radius to meters

            //// Filter the entities based on the distance from the user's location
            //return query.Where(e => e != null && e.Latitude != null && e.Longitude != null &&
            //                    new Point(e.Longitude.Value, e.Latitude.Value)
            //                    {
            //                        SRID = 4326  // Standard for lat/lon (WGS 84)
            //                    }.Distance(userLocation) <= radiusMeters);
            throw new NotImplementedException();
        }


        /// <summary>
        /// Sorts items by distance to a given point.
        /// </summary>
        public IQueryable<T> SortByDistance<T>(
            IQueryable<T> query,
            double latitude,
            double longitude,
            bool ascending = true)
            where T : ILocation
        {
            var center = LocationHelper.CreatePoint(latitude, longitude);

            return ascending
                ? query.OrderBy(x => x.Location != null ? x.Location.Distance(center) : double.MaxValue)
                : query.OrderByDescending(x => x.Location != null ? x.Location.Distance(center) : double.MinValue);
        }


        /// <summary>
        /// Does both Filtering and Sorting in one method
        /// </summary>
        public IQueryable<T> FilterAndSortByNearest<T>(
            IQueryable<T> query,
            double latitude,
            double longitude,
            int radiusKm,
            bool ascending = true)
            where T : ILocation
        {
            var filtered = FilterByRadius(query, latitude, longitude, radiusKm);
            return SortByDistance(filtered, latitude, longitude, ascending);
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
