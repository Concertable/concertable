using Application.DTOs;
using Core.Parameters;
using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;

namespace Application.Interfaces
{
    public interface IHeaderRepository<TEntity, TDto> : IRepository<TEntity> 
        where TDto : HeaderDto
        where TEntity: BaseEntity
    {
        Task<PaginationResponse<TDto>> GetHeadersAsync(SearchParams? searchParams);
        Task<IEnumerable<TDto>> GetHeadersAsync(int amount);
    }
}
