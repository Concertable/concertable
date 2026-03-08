using Application.DTOs;
using Core.Entities;

namespace Application.Mappers;

public static class MessageMappers
{
    public static MessageDto ToDto(this Message message) => new()
    {
        Id = message.Id,
        Content = message.Content,
        FromUser = message.FromUser.ToDto(),
        Action = !string.IsNullOrEmpty(message.Action) && message.ActionId.HasValue
            ? new ActionDto(message.Action, message.ActionId.Value)
            : null
    };

    public static IEnumerable<MessageDto> ToDtos(this IEnumerable<Message> messages) =>
        messages.Select(m => m.ToDto());
}
