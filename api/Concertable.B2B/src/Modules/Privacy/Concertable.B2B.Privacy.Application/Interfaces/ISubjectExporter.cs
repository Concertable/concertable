namespace Concertable.B2B.Privacy.Application.Interfaces;

/// <summary>Assembles the subject's portable data export from each B2B module's own facade fragment. A single
/// read-orchestration operation — no repository, no unit of work — so it is named for what it does, not
/// <c>Service</c>.</summary>
internal interface ISubjectExporter
{
    Task<SubjectExportBundle> ExportAsync(Guid subjectId, CancellationToken ct = default);
}
