using Application.DTOs;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPreferenceService
    {
        Task<PreferenceDto> GetByUserIdAsync(int userId);
        Task<PreferenceDto> GetByUserAsync();
        Task<IEnumerable<PreferenceDto>> GetAsync();
        Task<PreferenceDto> CreateAsync(CreatePreferenceDto createPreferenceceDto, int userId);
        Task<PreferenceDto> UpdateAsync(PreferenceDto preferenceDto);
    }

}
