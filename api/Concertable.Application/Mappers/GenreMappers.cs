using Concertable.Application.DTOs;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public static class GenreMappers
{
    public static GenreDto ToDto(this GenreEntity genre) => new(genre.Id, genre.Name);

    public static GenreEntity ToEntity(this GenreDto dto) => GenreEntity.Create(dto.Id, dto.Name);

    public static IEnumerable<GenreDto> ToDtos(this IEnumerable<GenreEntity> genres) =>
        genres.Select(g => g.ToDto());
}
