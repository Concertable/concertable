namespace Application.DTOs
{
    public record PreferenceDto
    {
        public int Id { get; set; }
        public UserDto User { get; set; }
        public int RadiusKm { get; set; }
        public IEnumerable<GenreDto> Genres { get; set; } = new List<GenreDto>();
    }
}
