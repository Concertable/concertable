using System.Text.Json.Serialization;

namespace Concertable.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageAction
{
    ApplicationReceived,
    ApplicationAccepted,
    ConcertPosted
}
