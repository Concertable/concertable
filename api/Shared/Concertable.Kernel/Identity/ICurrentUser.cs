namespace Concertable.Kernel.Identity;

public interface ICurrentUser
{
    Guid? Id { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }

    /// <summary>
    /// The opaque owner key for resources keyed by owner rather than user (e.g. Payment's payout account) —
    /// from the <c>owner</c> claim. For a B2B operator/artist this is their tenant id; absent for customers,
    /// who own resources as themselves (see <c>CurrentUserExtensions.GetOwnerId</c>, which falls back to <see cref="Id"/>).
    /// </summary>
    Guid? Owner { get; }
}
