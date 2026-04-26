using System.Text.Json.Serialization;

namespace Concertable.Messaging.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageAction
{
    ApplicationReceived,
    ApplicationAccepted,
    ConcertPosted
}
