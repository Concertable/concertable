using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Requests
{
    public record CreateArtistRequest
    {
        public string Name { get; set; }
        public string About { get; set; }
        public IEnumerable<GenreDto> Genres { get; set; }
    }

    public record ArtistCreateRequest
    {
        public CreateArtistRequest Artist { get; set; }
        public IFormFile Image { get; set; }
    }

    public record ArtistUpdateRequest
    {
        public ArtistDto Artist { get; set; }
        public IFormFile? Image { get; set; }
    }
}
