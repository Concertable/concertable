using Concertable.Application.DTOs;

namespace Concertable.Application.Interfaces;

public interface IMessageNotificationService
{
    Task MessageReceivedAsync(string userId, MessageDto payload);
}
