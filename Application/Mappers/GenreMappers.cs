using Application.DTOs;
using Core.Entities;

namespace Application.Mappers
{
    public static class GenreMappers
    {
        public static GenreDto ToDto(this Genre genre) => new()
        {
            Id = genre.Id,
            Name = genre.Name
        };

        public static Genre ToEntity(this GenreDto dto) => new()
        {
            Id = dto.Id,
            Name = dto.Name
        };

        public static IEnumerable<GenreDto> ToDtos(this IEnumerable<Genre> genres) =>
            genres.Select(g => g.ToDto());
    }
}
