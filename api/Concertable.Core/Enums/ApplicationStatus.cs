using System.Text.Json.Serialization;

namespace Concertable.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApplicationStatus
{
    Pending,
    Rejected,
    Withdrawn,
    AwaitingPayment,
    Confirmed,
    Complete,
    Settled
}
