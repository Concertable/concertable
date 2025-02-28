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
        IEnumerable<T> FilterByRadius<T>(double latitude, double longitude, double radiusKm, IEnumerable<T> items)
            where T : ILocation;

        IEnumerable<T> SortByDistance<T>(double latitude, double longitude, IEnumerable<T> items, bool ascending = true)
            where T : ILocation;

        bool IsWithinRadius(double lat1, double lon1, double lat2, double lon2, int radiusKm);
    }
}
