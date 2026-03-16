using Core.Entities;
using Application.Interfaces;
using Application.DTOs;
using Application.Mappers;

namespace Infrastructure.Services;

public class GenreService : IGenreService
{
    private readonly IGenreRepository genreRepository;

    public GenreService(IGenreRepository genreRepository)
    {
        this.genreRepository = genreRepository;
    }

    public async Task<IEnumerable<GenreDto>> GetAllAsync()
    {
        var genres = await genreRepository.GetAllAsync();
        return genres.ToDtos();
    }
}
