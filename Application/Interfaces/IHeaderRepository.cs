using Application.DTOs;
using Core.Parameters;
using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Application.Interfaces
{
    public interface IHeaderRepository<TEntity, TDto> : IRepository<TEntity> 
        where TDto : HeaderDto
        where TEntity: BaseEntity
    {
        Task<PaginationResponse<TDto>> GetRawHeadersAsync(SearchParams? searchParams);

    }
}
