using System.Text.Json.Serialization;

namespace Concertable.Messaging.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageAction
{
    ApplicationReceived,
    ApplicationAccepted,
    ConcertPosted
}
