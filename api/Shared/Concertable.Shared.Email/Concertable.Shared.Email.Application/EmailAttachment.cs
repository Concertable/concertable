namespace Concertable.Shared.Email;

public sealed record EmailAttachment(byte[] Content, string FileName, string MimeType = "application/pdf");
