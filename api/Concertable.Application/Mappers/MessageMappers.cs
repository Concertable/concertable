using Concertable.Application.DTOs;
using Concertable.Messaging.Domain;

namespace Concertable.Application.Mappers;

public static class MessageMappers
{
    public static MessageDto ToDto(this MessageEntity message, UserDto fromUser) => new()
    {
        Id = message.Id,
        Content = message.Content,
        FromUser = fromUser,
        Action = message.Action
    };
}
