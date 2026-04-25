using System.Text.Json.Serialization;

namespace Concertable.Contract.Abstractions;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentMethod
{
    Cash,
    Transfer
}
