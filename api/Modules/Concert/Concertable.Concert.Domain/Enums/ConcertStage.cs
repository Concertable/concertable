using System.Text.Json.Serialization;

namespace Concertable.Concert.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConcertStage
{
    None = 0,
    Applied = 1,
    CheckedOut = 2,
    Verified = 3,
    Accepted = 4,
    Settled = 5,
    Finished = 6
}
