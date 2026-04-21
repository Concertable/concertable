using Concertable.Application.DTOs;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces;

public interface IReviewService
{
    Task<IPagination<ReviewDto>> GetAsync(int id, IPageParams pageParams);
    Task<ReviewSummaryDto> GetSummaryAsync(int id);
    Task<ReviewDto> CreateAsync(CreateReviewRequest request);
}
