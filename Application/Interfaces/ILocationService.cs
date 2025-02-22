using Application.DTOs;
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
        IEnumerable<T> FilterAndSortByNearest<T>(SearchParams searchParams, IEnumerable<T> headersDto)
            where T : HeaderDto;
    }
}
