namespace Application.Requests
{
    public record MarkMessagesReadRequest
    {
        public List<int> MessageIds { get; set; }
    }
}
