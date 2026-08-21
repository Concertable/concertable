namespace Concertable.B2B.Privacy.Application.DTOs;

/// <summary>The subject's portable data export (GDPR arts. 15/20): one machine-readable bundle assembled from
/// each B2B module's own fragment via its facade. The Auth identity fragment and the Payment financial fragment
/// join in Phases 2 and 4.</summary>
public sealed record SubjectExportBundle
{
    public required Guid SubjectId { get; init; }
    public UserExport? User { get; init; }
    public IReadOnlyList<MembershipDto> Memberships { get; init; } = [];
    public IReadOnlyList<MessageExport> Messages { get; init; } = [];
    public ConcertRecordsExport ConcertRecords { get; init; } = new();
}
