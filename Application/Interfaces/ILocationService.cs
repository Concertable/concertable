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
        bool IsWithinRadius(double? lat1, double? lon1, double lat2, double lon2, int radiusKm);
    }
}

