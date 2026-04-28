
namespace Concertable.User.Infrastructure.Extensions;

internal static class ManagerMappers
{
    public static ManagerDto ToDto(this ManagerEntity manager) => new()
    {
        Id = manager.Id,
        Email = manager.Email,
        Avatar = manager.Avatar
    };
}
