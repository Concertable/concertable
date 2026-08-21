namespace Concertable.B2B.Privacy.Infrastructure.Services;

internal sealed class SubjectExporter : ISubjectExporter
{
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

    public async Task<SubjectExportBundle> ExportAsync(Guid subjectId, CancellationToken ct = default)
    {
        var user = await userModule.ExportAsync(subjectId, ct);
        var memberships = await tenantModule.GetMembershipsAsync(subjectId, ct);
        var tenantIds = memberships.Select(m => m.TenantId).Distinct().ToArray();
        var messages = await conversationsModule.ExportAsync(subjectId, ct);
        var concertRecords = await concertModule.ExportAsync(tenantIds, ct);

        return new SubjectExportBundle
        {
            SubjectId = subjectId,
            User = user.Match<UserExport?>(u => u, () => null),
            Memberships = memberships,
            Messages = messages,
            ConcertRecords = concertRecords,
        };
    }
}
