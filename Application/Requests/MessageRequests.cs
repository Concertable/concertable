using System.ComponentModel.DataAnnotations;

namespace Application.Requests
{
    public record MarkMessagesReadRequest
    {
        [MinLength(1, ErrorMessage = "Require one MessageId minimum")]
        public List<int> MessageIds { get; set; }
    }
}
