namespace Concertable.Kernel.Identity;

public static class CurrentUserExtensions
{
    public static Guid GetId(this ICurrentUser currentUser) =>
        currentUser.Id ?? throw new UnauthorizedAccessException("User not authenticated.");

    /// <summary>
    /// The opaque owner key for owner-scoped resources: the <c>owner</c> claim (a B2B tenant id) when present,
    /// otherwise the caller's own user id (customers own as themselves). Payment keys payout accounts by this.
    /// </summary>
    public static Guid GetOwnerId(this ICurrentUser currentUser) =>
        currentUser.Owner ?? currentUser.GetId();
}
