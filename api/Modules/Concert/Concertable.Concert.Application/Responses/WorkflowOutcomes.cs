using System.Text.Json.Serialization;

namespace Concertable.Concert.Application.Responses;

[JsonDerivedType(typeof(ImmediateAcceptOutcome), "immediate")]
[JsonDerivedType(typeof(DeferredAcceptOutcome), "deferred")]
internal interface IAcceptOutcome { }

internal record ImmediateAcceptOutcome(PaymentResponse Payment) : IAcceptOutcome;
internal record DeferredAcceptOutcome : IAcceptOutcome;
