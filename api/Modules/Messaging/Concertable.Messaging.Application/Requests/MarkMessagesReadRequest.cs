namespace Concertable.Messaging.Application.Requests;

internal record MarkMessagesReadRequest
{
    public required List<int> MessageIds { get; set; }
}
