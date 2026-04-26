using Concertable.Application.DTOs;

namespace Concertable.Application.Interfaces;

public interface IMessageNotifier
{
    Task MessageReceivedAsync(string userId, MessageDto payload);
}
