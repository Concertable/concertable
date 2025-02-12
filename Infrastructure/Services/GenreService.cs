using Core.Entities;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using AutoMapper;

namespace Infrastructure.Services
{
    public class GenreService : IGenreService
    {
        private readonly IGenreRepository genreRepository;
        private readonly IMapper mapper;

        public GenreService(IGenreRepository genreRepository, IMapper mapper) 
        {
            this.genreRepository = genreRepository;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<GenreDto>> GetAllAsync()
        {
            var genres = await genreRepository.GetAllAsync();
            return mapper.Map<IEnumerable<GenreDto>>(genres);
        }
    }
}
