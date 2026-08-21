using System.Text.Json;

namespace Concertable.B2B.Privacy.Infrastructure.Services;

internal sealed class SubjectExporter : ISubjectExporter
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly IUserModule userModule;
    private readonly ITenantModule tenantModule;
    private readonly IConversationsModule conversationsModule;
    private readonly IConcertModule concertModule;

    public SubjectExporter(
        IUserModule userModule,
        ITenantModule tenantModule,
        IConversationsModule conversationsModule,
        IConcertModule concertModule)
    {
        this.userModule = userModule;
        this.tenantModule = tenantModule;
        this.conversationsModule = conversationsModule;
        this.concertModule = concertModule;
    }

    public async Task<FileDownload> ExportAsync(Guid subjectId, CancellationToken ct = default)
    {
        var user = await userModule.ExportAsync(subjectId, ct);
        var memberships = await tenantModule.GetMembershipsAsync(subjectId, ct);
        var tenantIds = memberships.Select(m => m.TenantId).Distinct().ToArray();
        var messages = await conversationsModule.ExportMessagesAsync(subjectId, ct);
        var concertRecords = await concertModule.ExportRecordsAsync(tenantIds, ct);

        var payload = new
        {
            subjectId,
            user = user.Match<UserExport?>(u => u, () => null),
            memberships,
            messages,
            concertRecords,
        };

        var content = JsonSerializer.SerializeToUtf8Bytes(payload, SerializerOptions);
        return new FileDownload(content, $"subject-export-{subjectId:N}.json", "application/json");
    }
}
