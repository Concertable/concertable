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
        private readonly IGeometryService geometryService;

        public LocationService(IGeometryService geometryService)
        {
            this.geometryService = geometryService;
        }

        public bool IsWithinRadius(double? lat1, double? lon1, double lat2, double lon2, int radiusKm)
        {
            if (!lat1.HasValue || !lon1.HasValue)
                return false;

            var geo1 = new GeoCoordinate(lat1.Value, lon1.Value);
            var geo2 = new GeoCoordinate(lat2, lon2);

            var distanceMeters = geo1.GetDistanceTo(geo2);

            return distanceMeters <= radiusKm * 1000;
        }

    }
}
