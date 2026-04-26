using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Shared;

namespace Concertable.Shared.Infrastructure.Services;

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
