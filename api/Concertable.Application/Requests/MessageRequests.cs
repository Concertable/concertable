namespace Application.Requests;

public record MarkMessagesReadRequest
{
    public required List<int> MessageIds { get; set; }
}
