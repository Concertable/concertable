using Concertable.Application.DTOs;
using Concertable.Application.Requests;

namespace Concertable.Application.Interfaces;

public interface IPreferenceService
{
    Task<PreferenceDto?> GetByUserIdAsync(Guid userId);
    Task<PreferenceDto?> GetByUserAsync();
    Task<IEnumerable<PreferenceDto>> GetAsync();
    Task<PreferenceDto> CreateAsync(CreatePreferenceRequest request, Guid? userId = null);
    Task<PreferenceDto> UpdateAsync(PreferenceDto preferenceDto);
}
