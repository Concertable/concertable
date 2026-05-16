namespace Concertable.User.Infrastructure.Data;

internal static class UserQueryExtensions
{
    public static IQueryable<UserEntity> WhereCredentials(this IQueryable<UserEntity> users, string email, Role? role) =>
        role is { } r
            ? users.Where(u => u.Email == email && u.Role == r)
            : users.Where(u => u.Email == email);
}
