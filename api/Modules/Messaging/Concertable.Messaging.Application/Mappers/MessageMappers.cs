using Concertable.Messaging.Application.DTOs;

namespace Concertable.Messaging.Application.Mappers;

internal static class MessageMappers
{
    public static MessageDto ToDto(this MessageEntity message, MessageUserDto fromUser) => new()
    {
        Id = message.Id,
        Content = message.Content,
        FromUser = fromUser,
        Action = message.Action
    };

    public static MessageUserDto ToMessageUserDto(this IUser user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Role = user.Role,
        Latitude = user.Latitude,
        Longitude = user.Longitude,
        County = user.County,
        Town = user.Town
    };
}
