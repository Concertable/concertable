using Application.DTOs;
using Core.Interfaces;
using Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ILocationService
    {
        IQueryable<T> FilterByRadius<T>(IQueryable<T> query, double latitude, double longitude, double radiusKm) where T : ILocation;
        IQueryable<T> SortByDistance<T>(IQueryable<T> query,  double latitude, double longitude, bool ascending = true) where T : ILocation;
        IQueryable<T> FilterAndSortByNearest<T>(IQueryable<T> query,  double latitude, double longitude, int radiusKm, bool ascending = true) where T : ILocation;
        bool IsWithinRadius(double lat1, double lon1, double lat2, double lon2, int radiusKm);
    }
}

