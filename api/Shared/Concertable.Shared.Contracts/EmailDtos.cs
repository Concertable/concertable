namespace Concertable.Application.DTOs;

public record AttachmentDto
{
    public required byte[] Content { get; set; }
    public required string FileName { get; set; }
    public string MimeType { get; set; } = "application/pdf";
}

public record EmailDto
{
    public required string To { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public IEnumerable<AttachmentDto> Attachments { get; set; } = [];
}
