using System.Net;
using Concertable.B2B.IntegrationTests.Fixtures;
using Concertable.B2B.Privacy.Application.Interfaces;
using Concertable.B2B.Privacy.Domain.Lifecycle;
using Concertable.B2B.Tenant.Contracts;
using Concertable.B2B.User.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Concertable.B2B.Privacy.IntegrationTests;

[Collection("Integration")]
public sealed class SubjectRightsApiTests : IAsyncLifetime
{
    private readonly PrivacyApiFixture fixture;

    public SubjectRightsApiTests(PrivacyApiFixture fixture, ITestOutputHelper output)
    {
        this.fixture = fixture;
        fixture.AttachOutput(output);
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() { fixture.DetachOutput(); return Task.CompletedTask; }

    private async Task<T> InScopeAsync<T>(Func<IServiceProvider, Task<T>> body)
    {
        using var scope = fixture.Services.CreateScope();
        return await body(scope.ServiceProvider);
    }

    #region RequestErasure

    [Fact]
    public async Task RequestErasure_CleanSubject_AnonymisesAndCompletes()
    {
        // ArtistManagerNoArtist registered but never set up an organisation: a tenant it solely owns, no
        // concerts, so no live obligation — the gate clears and erasure runs to completion.
        var subject = fixture.SeedState.ArtistManagerNoArtist;

        var result = await InScopeAsync(sp =>
            sp.GetRequiredService<ISubjectErasureService>().RequestErasureAsync(subject.Id));

        Assert.Equal(ErasureState.Completed, result.State);

        var memberships = await InScopeAsync(sp =>
            sp.GetRequiredService<ITenantModule>().GetMembershipsAsync(subject.Id));
        Assert.Empty(memberships);

        var user = await InScopeAsync(sp =>
            sp.GetRequiredService<IUserModule>().ExportAsync(subject.Id));
        Assert.True(user.TryGetValue(out var fragment));
        Assert.Contains("erased", fragment.Email);
    }

    [Fact]
    public async Task RequestErasure_SubjectWithLiveObligation_DefersAndTouchesNothing()
    {
        // VenueManager1's tenant is the venue party to the seeded Accepted (payment-pending) booking — a live
        // financial obligation — so erasure must fail closed to Deferred and leave every row intact.
        var subject = fixture.SeedState.VenueManager1;

        var result = await InScopeAsync(sp =>
            sp.GetRequiredService<ISubjectErasureService>().RequestErasureAsync(subject.Id));

        Assert.Equal(ErasureState.Deferred, result.State);
        Assert.NotNull(result.DeferralReason);

        var memberships = await InScopeAsync(sp =>
            sp.GetRequiredService<ITenantModule>().GetMembershipsAsync(subject.Id));
        Assert.NotEmpty(memberships);

        var user = await InScopeAsync(sp =>
            sp.GetRequiredService<IUserModule>().ExportAsync(subject.Id));
        Assert.True(user.TryGetValue(out var fragment));
        Assert.DoesNotContain("erased", fragment.Email);
    }

    [Fact]
    public async Task RequestErasure_Route_IsReachableForAdminAndForbiddenOtherwise()
    {
        var subjectId = Guid.NewGuid();
        var admin = fixture.CreateClient(fixture.SeedState.Admin);
        var nonAdmin = fixture.CreateClient(fixture.SeedState.VenueManager2);

        var allowed = await admin.PostAsync($"/api/subject-erasure/{subjectId}", null);
        var forbidden = await nonAdmin.PostAsync($"/api/subject-erasure/{subjectId}", null);

        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    #endregion

    #region Export

    [Fact]
    public async Task Export_SubjectWithData_ReturnsExactlyTheirBundle()
    {
        var subject = fixture.SeedState.VenueManager1;

        var bundle = await InScopeAsync(sp =>
            sp.GetRequiredService<ISubjectExporter>().ExportAsync(subject.Id));

        Assert.Equal(subject.Id, bundle.SubjectId);
        Assert.NotNull(bundle.User);
        Assert.NotEmpty(bundle.Memberships);
    }

    #endregion
}
