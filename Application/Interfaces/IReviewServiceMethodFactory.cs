using Application.DTOs;
using Application.Responses;
using Core.Parameters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IReviewServiceMethodFactory : IScopeDisposable
    {
        Func<int, PaginationParams, Task<PaginationResponse<ReviewDto>>> GetMethod(string entityType);
    }
}
