using System.Text.Json.Serialization;

namespace Concertable.Application.Responses;

[JsonDerivedType(typeof(ImmediateAcceptOutcome), "immediate")]
[JsonDerivedType(typeof(DeferredAcceptOutcome), "deferred")]
public interface IAcceptOutcome { }

public record ImmediateAcceptOutcome(PaymentResponse Payment) : IAcceptOutcome;
public record DeferredAcceptOutcome : IAcceptOutcome;

[JsonDerivedType(typeof(ImmediateFinishOutcome), "immediate")]
[JsonDerivedType(typeof(DeferredFinishOutcome), "deferred")]
public interface IFinishOutcome { }

public record ImmediateFinishOutcome : IFinishOutcome;
public record DeferredFinishOutcome(PaymentResponse Payment) : IFinishOutcome;
