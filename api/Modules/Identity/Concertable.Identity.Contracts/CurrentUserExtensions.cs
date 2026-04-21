namespace Concertable.Identity.Contracts;

public static class CurrentUserExtensions
{
    public static Guid GetId(this ICurrentUser currentUser) =>
        currentUser.Id ?? throw new UnauthorizedAccessException("User not authenticated.");

    public static Role GetRole(this ICurrentUser currentUser) =>
        currentUser.Role ?? throw new UnauthorizedAccessException("User not authenticated.");
}
