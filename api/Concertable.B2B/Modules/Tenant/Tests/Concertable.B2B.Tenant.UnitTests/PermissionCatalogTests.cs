using System.Reflection;
using Concertable.B2B.Tenant.Contracts;

namespace Concertable.B2B.Tenant.UnitTests;

/// <summary>
/// Recovers the compile-time guarantee an enum would have given (§1.3): every declared permission constant is
/// granted to at least one role within its catalog — so a typo'd or orphaned permission string fails the
/// build's tests, not silently 403s in production. Plus the headline guarantee of the persona split: a
/// persona-exclusive permission is unreachable for the other persona by construction.
/// </summary>
public sealed class PermissionCatalogTests
{
    private static readonly IPermissionCatalog Catalog = Build();

    private static IPermissionCatalog Build()
    {
        var shared = new SharedPermissions();
        return new PermissionCatalog(new VenuePermissions(shared), new ArtistPermissions(shared));
    }

    private static IReadOnlySet<string> ConstantsOf(Type type) =>
        type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToHashSet();

    [Fact]
    public void EveryDeclaredConstant_IsGrantedToSomeRole()
    {
        var shared = new SharedPermissions();
        AssertEachConstantGranted(typeof(SharedPermissions), shared);
        AssertEachConstantGranted(typeof(VenuePermissions), new VenuePermissions(shared));
        AssertEachConstantGranted(typeof(ArtistPermissions), new ArtistPermissions(shared));
    }

    private static void AssertEachConstantGranted(Type holder, IPermissionSet set)
    {
        foreach (var permission in ConstantsOf(holder))
            Assert.True(
                Enum.GetValues<TenantRole>().Any(role => set.Grants(role, permission)),
                $"{holder.Name}: declared but granted to no role: {permission}");
    }

    [Fact]
    public void Owner_HoldsEveryPermissionOfItsPersona()
    {
        var shared = new SharedPermissions();
        var venue = new VenuePermissions(shared);
        var artist = new ArtistPermissions(shared);

        foreach (var permission in ConstantsOf(typeof(SharedPermissions)).Concat(ConstantsOf(typeof(VenuePermissions))))
            Assert.True(venue.Grants(TenantRole.Owner, permission), $"Venue Owner is missing {permission}");

        foreach (var permission in ConstantsOf(typeof(SharedPermissions)).Concat(ConstantsOf(typeof(ArtistPermissions))))
            Assert.True(artist.Grants(TenantRole.Owner, permission), $"Artist Owner is missing {permission}");
    }

    [Fact]
    public void PersonaExclusivePermissions_AreUnreachableForTheOtherPersona()
    {
        var shared = new SharedPermissions();
        var venue = new VenuePermissions(shared);
        var artist = new ArtistPermissions(shared);

        foreach (var role in Enum.GetValues<TenantRole>())
        {
            foreach (var artistOnly in ConstantsOf(typeof(ArtistPermissions)))
                Assert.False(venue.Grants(role, artistOnly), $"Venue {role} was granted artist-only {artistOnly}");

            foreach (var venueOnly in ConstantsOf(typeof(VenuePermissions)))
                Assert.False(artist.Grants(role, venueOnly), $"Artist {role} was granted venue-only {venueOnly}");
        }
    }

    [Theory]
    [InlineData(TenantType.Venue, TenantRole.Finance, SharedPermissions.PayoutsManage, true)]
    [InlineData(TenantType.Venue, TenantRole.Finance, SharedPermissions.SettlementTrigger, true)]
    [InlineData(TenantType.Venue, TenantRole.Finance, SharedPermissions.ProfileEdit, false)]
    [InlineData(TenantType.Venue, TenantRole.Manager, VenuePermissions.OpportunitiesManage, true)]
    [InlineData(TenantType.Venue, TenantRole.Manager, SharedPermissions.PayoutsManage, false)]
    [InlineData(TenantType.Venue, TenantRole.Manager, SharedPermissions.TenantDelete, false)]
    [InlineData(TenantType.Venue, TenantRole.Staff, SharedPermissions.MessagesSend, true)]
    [InlineData(TenantType.Venue, TenantRole.Staff, SharedPermissions.ProfileEdit, false)]
    [InlineData(TenantType.Venue, TenantRole.Door, VenuePermissions.ConcertsCheckIn, true)]
    [InlineData(TenantType.Venue, TenantRole.Door, SharedPermissions.ConcertsOpsEdit, false)]
    [InlineData(TenantType.Venue, TenantRole.Sound, SharedPermissions.ConcertsOpsEdit, true)]
    [InlineData(TenantType.Venue, TenantRole.Sound, VenuePermissions.ConcertsCheckIn, false)]
    [InlineData(TenantType.Artist, TenantRole.Owner, ArtistPermissions.ApplicationsSubmit, true)]
    [InlineData(TenantType.Venue, TenantRole.Owner, ArtistPermissions.ApplicationsSubmit, false)]
    [InlineData(TenantType.Artist, TenantRole.Owner, VenuePermissions.ApplicationsDecide, false)]
    public void Grants_MatchesMatrix(TenantType persona, TenantRole role, string permission, bool expected) =>
        Assert.Equal(expected, Catalog.Grants(persona, role, permission));

    [Theory]
    [InlineData(TenantType.Venue, TenantRole.Door)]
    [InlineData(TenantType.Venue, TenantRole.Sound)]
    [InlineData(TenantType.Artist, TenantRole.Door)]
    [InlineData(TenantType.Artist, TenantRole.Sound)]
    public void ReservedRoles_AlwaysHaveOperationsView(TenantType persona, TenantRole role) =>
        Assert.True(Catalog.Grants(persona, role, SharedPermissions.OperationsView));
}
