using Concertable.Application.DTOs;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public static class MessageMappers
{
    public static MessageDto ToDto(this MessageEntity message) => new()
    {
        Id = message.Id,
        Content = message.Content,
        FromUser = message.FromUser.ToDto(),
        Action = !string.IsNullOrEmpty(message.Action) && message.ActionId.HasValue
            ? new ActionDto(message.Action, message.ActionId.Value)
            : null
    };

    public static IEnumerable<MessageDto> ToDtos(this IEnumerable<MessageEntity> messages) =>
        messages.Select(m => m.ToDto());
}
