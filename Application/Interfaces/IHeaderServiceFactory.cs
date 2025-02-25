using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IHeaderServiceFactory
    {
        IHeaderService<TDto> GetService<TDto>(string entityType) where TDto : HeaderDto;
        void CreateScope();
        void DisposeScope();
    }
}
