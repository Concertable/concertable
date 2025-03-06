using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Core.Entities;
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
        private readonly IAuthService authService;
        private readonly IMapper mapper;

        public PreferenceService(
            IPreferenceRepository preferenceRepository,
            IAuthService authService,
            IMapper mapper)
        {
            this.preferenceRepository = preferenceRepository;  
            this.authService = authService;
            this.mapper = mapper;
        }

        public async Task<PreferenceDto> CreateAsync(CreatePreferenceDto createPreferenceDto)
        {
            var preference = mapper.Map<Preference>(createPreferenceDto);

            var user = await authService.GetCurrentUserAsync();

            foreach(var genreDto in createPreferenceDto.Genres)
            {
                preference.GenrePreferences.Add(new GenrePreference
                {
                    PreferenceId = preference.Id,
                    GenreId = genreDto.Id
                });
            }

            preference.UserId = user.Id;

            preferenceRepository.Update(preference);
            await preferenceRepository.SaveChangesAsync();

            return mapper.Map<PreferenceDto>(preference);
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

        public async Task<PreferenceDto> UpdateAsync(PreferenceDto preferenceDto)
        {
            var preference = await preferenceRepository.GetByIdAsync(preferenceDto.Id);

            var userId = (await authService.GetCurrentUserAsync()).Id;

            if (userId != preference.User.Id)
                throw new UnauthorizedAccessException("You do not own this preference");

            preference.GenrePreferences.Clear();
            foreach (var genreDto in preferenceDto.Genres)
            {
                preference.GenrePreferences.Add(new GenrePreference
                {
                    PreferenceId = preference.Id,
                    GenreId = genreDto.Id
                });
            }

            preferenceRepository.Update(preference);
            await preferenceRepository.SaveChangesAsync();

            return mapper.Map<PreferenceDto>(preference);
        }
    }
}
