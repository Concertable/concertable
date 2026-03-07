using Application.DTOs;
using Core.Entities;

namespace Infrastructure.Mappers
{
    public static class GenreMappers
    {
        public static GenreDto ToDto(this Genre genre) => new GenreDto
        {
            Id = genre.Id,
            Name = genre.Name
        };

        public static Genre ToEntity(this GenreDto dto) => new Genre
        {
            Id = dto.Id,
            Name = dto.Name
        };
    }
}
