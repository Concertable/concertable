namespace Concertable.Shared.Email.Infrastructure;

internal sealed record AttachmentDto
{
    public required byte[] Content { get; set; }
    public required string FileName { get; set; }
    public string MimeType { get; set; } = "application/pdf";
}

internal sealed record EmailDto
{
    public required string To { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public IReadOnlyList<AttachmentDto> Attachments { get; set; } = [];
}
