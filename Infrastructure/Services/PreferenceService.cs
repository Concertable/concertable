using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class PreferenceService : IPreferenceService
    {
        private readonly IPreferenceRepository preferenceRepository;
        private readonly IMapper mapper;

        public PreferenceService(IPreferenceRepository preferenceRepository, IMapper mapper)
        {
            this.preferenceRepository = preferenceRepository;  
            this.mapper = mapper;
        }

        public async Task<IEnumerable<PreferenceDto>> GetAsync()
        {
            var preferences = await preferenceRepository.GetAllAsync();

            return mapper.Map<IEnumerable<PreferenceDto>>(preferences);
        }

        public async Task<PreferenceDto> GetByUserIdAsync(int userId)
        {
            var preference = await preferenceRepository.GetByUserIdAsync(userId);

            return mapper.Map<PreferenceDto>(preference);
        }
    }
}
