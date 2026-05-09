using System.Text.Json.Serialization;
using Concertable.Payment.Contracts;

namespace Concertable.Concert.Application.Responses;

[JsonDerivedType(typeof(ImmediateAcceptOutcome), "immediate")]
[JsonDerivedType(typeof(DeferredAcceptOutcome), "deferred")]
internal interface IAcceptOutcome { }

internal record ImmediateAcceptOutcome(PaymentResponse Payment) : IAcceptOutcome;
internal record DeferredAcceptOutcome : IAcceptOutcome;
