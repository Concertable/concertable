using Concertable.Artist.Application.DTOs;
using Concertable.Web.Responses;

namespace Concertable.Web.Mappers;

public static class ArtistResponseMappers
{
    public static ArtistDetailsResponse ToDetailsResponse(this ArtistDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        About = dto.About,
        BannerUrl = dto.BannerUrl,
        Avatar = dto.Avatar,
        Rating = dto.Rating,
        Genres = dto.Genres,
        County = dto.County,
        Town = dto.Town,
        Email = dto.Email
    };
}
