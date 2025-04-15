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

        public bool IsWithinRadius(double? lat1, double? lon1,  double? lat2, double? lon2, int radiusKm)
        {
            var point1 = geometryService.CreatePoint(lat1, lon1);
            var point2 = geometryService.CreatePoint(lat2, lon2);

            if (point1 is null || point2 is null)
                return false;

            var distanceMeters = point1!.Distance(point2);
            return distanceMeters <= (radiusKm * 1000);
        }
    }
}
