using Application.DTOs;

namespace Application.Responses
{
    public record ConcertPostResponse
    {
        public required ConcertDto Concert { get; set; }
        public required ConcertHeaderDto ConcertHeader { get; set; }
        public IEnumerable<int> UserIds { get; set; } = [];
    }
}
