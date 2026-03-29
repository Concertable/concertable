using Concertable.Core.Entities;
using Concertable.Application.Interfaces;
using Concertable.Application.DTOs;
using Concertable.Application.Mappers;

namespace Concertable.Infrastructure.Services;

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
