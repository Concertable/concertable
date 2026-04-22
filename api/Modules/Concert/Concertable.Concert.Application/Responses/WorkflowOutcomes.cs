using System.Text.Json.Serialization;

namespace Concertable.Concert.Application.Responses;

[JsonDerivedType(typeof(ImmediateAcceptOutcome), "immediate")]
[JsonDerivedType(typeof(DeferredAcceptOutcome), "deferred")]
internal interface IAcceptOutcome { }

internal record ImmediateAcceptOutcome(PaymentResponse Payment) : IAcceptOutcome;
internal record DeferredAcceptOutcome : IAcceptOutcome;

[JsonDerivedType(typeof(ImmediateFinishOutcome), "immediate")]
[JsonDerivedType(typeof(DeferredFinishOutcome), "deferred")]
internal interface IFinishOutcome { }

internal record ImmediateFinishOutcome : IFinishOutcome;
internal record DeferredFinishOutcome(PaymentResponse Payment) : IFinishOutcome;
