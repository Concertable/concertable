using Application.DTOs;
using Core.Parameters;
using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IHeaderRepository<TDto> where TDto : HeaderDto
    {
        Task<PaginationResponse<TDto>> GetRawHeadersAsync(SearchParams? searchParams);
    }
}
