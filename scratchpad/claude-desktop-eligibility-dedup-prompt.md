I have a small C# code-duplication problem in a .NET backend that uses the Reunion Result-carrier library (`Result<TValue,TError>`, `UnitResult<TError>`, `Option<T>`, `ValidationResult`, with real extension methods `OrFailure`, `Ensure`/`EnsureAsync`, `Map`, `Bind`, `MapError`, `TryGetValue`, `TryGetError`). I want a genuinely correct, decisive design answer — not a menu of tradeoffs.

## The duplication

Two private methods in two different, already-existing classes in the same module run the **identical** sequence of calls:

**Call site A** — `ApplicationService.CheckCanAcceptAsync(ApplicationEntity application, CancellationToken ct)`. Only needs success/failure.
```csharp
private async Task<UnitResult<ApplicationEligibilityError>> CheckCanAcceptAsync(
    ApplicationEntity application,
    CancellationToken ct = default)
{
    var opportunityOption = await opportunityModule.GetAsync(application.OpportunityId, ct);
    if (!opportunityOption.TryGetValue(out var opportunity))
        return new ApplicationEligibilityError.OpportunityNotFound();

    var validation = await validator.CanAcceptAsync(opportunity, application);
    return validation.TryGetErrors(out var errors)
        ? new ApplicationEligibilityError.Invalid(new ValidationErrors(errors.ToDictionary()))
        : new Success();
}
```

**Call site B** — inside `ApplicationWorkflow.AcceptCoreAsync(...)`. Needs to KEEP the fetched `OpportunityDto` afterward (used later for deal/artist/venue/terms-fingerprint resolution), and wraps the failure in a DIFFERENT operation's error union:
```csharp
var opportunityOption = await opportunityModule.GetAsync(application.OpportunityId, ct);
if (!opportunityOption.TryGetValue(out var opportunity))
    return new AcceptApplicationError.Ineligible(new ApplicationEligibilityError.OpportunityNotFound());

var validation = await validator.CanAcceptAsync(opportunity, application);
if (validation.TryGetErrors(out var errors))
    return new AcceptApplicationError.Ineligible(
        new ApplicationEligibilityError.Invalid(new ValidationErrors(errors.ToDictionary())));
// ...continues using `opportunity` for further work below...
```

Both classes already have `IOpportunityModule opportunityModule` and `IApplicationValidator validator` constructor-injected (for other, unrelated purposes too — `opportunityModule` and `validator` are each used elsewhere in both classes for different operations, so neither dependency can simply be removed from either class).

`ApplicationEligibilityError` is a closed union (Dunet, `[Union(EnableImplicitConversions = false)]`) with cases `MissingArtist`, `OpportunityNotFound`, `ApplicationNotFound`, `Invalid(ValidationErrors Errors)`.

`AcceptApplicationError` is a different closed union for the Accept operation specifically, with (among other cases) `Ineligible(ApplicationEligibilityError Error)` wrapping the first union.

`IApplicationValidator` already has exactly two methods, both taking already-loaded data and returning bare `Task<ValidationResult>`: `CanApplyAsync(OpportunityDto, int artistId)` and `CanAcceptAsync(OpportunityDto, ApplicationEntity)`. It has no dependency on `IOpportunityModule` today (its only current dependency that isn't kernel/BCL is a read-projection over the Application module's own data — it does not currently reach into another module).

## What I actually want

One real, single implementation of "fetch the opportunity, run it through the validator, produce a Result the caller can use" — reused by both call sites — where:
- Call site A ends up with `UnitResult<ApplicationEligibilityError>` (discards the opportunity value).
- Call site B ends up with `Result<OpportunityDto, AcceptApplicationError>` (keeps the opportunity value, wraps the failure in its own union via `AcceptApplicationError.Ineligible(...)`).

## Options already proposed and rejected in an earlier, very long conversation — please don't re-propose any of these as-is, and please don't just tell me "the codebase already does X so it's fine" — I explicitly don't want precedent-matching, I want the actually correct general C#/.NET design

1. A static class/method taking the DI-resolved `IOpportunityModule`/`IApplicationValidator` as plain parameters (even made generic with a `Func<ApplicationEligibilityError, TError>` mapping delegate parameter) — rejected as "defeats the whole point of DI," not idiomatic.
2. A new constructor-injected, DI-registered class/interface with exactly one (possibly generic) method, e.g. `IAcceptEligibilityResolver.ResolveAsync<TError>(ApplicationEntity, Func<ApplicationEligibilityError,TError>, CancellationToken)` — rejected, though not for a clearly articulated technical reason beyond "that's not a Resolver" / general dissatisfaction with the shape. (For what it's worth, I'd already grounded the "Resolver" name in real BCL precedent — `System.Runtime.Loader.AssemblyDependencyResolver`, `IDependencyResolver` — as the conventional .NET agent-noun for "fetch by key, apply a rule, produce value-or-failure," but the name itself was rejected too, not just accepted-but-renamed.)
3. Adding this as a new member on `IApplicationWorkflow` (an existing, already multi-method, already-injected-elsewhere interface whose current two methods, `ApplyAsync`/`AcceptAsync`, are named lifecycle operations that actually advance a state machine) — rejected because a "check without executing" preview query doesn't belong on an interface whose contract is specifically state-advancing lifecycle operations.
4. Adding this as a new member on the existing `IApplicationValidator` (already multi-method: `CanApplyAsync`, `CanAcceptAsync`) — rejected because it would (a) require giving the validator a first-ever cross-module dependency (`IOpportunityModule`) purely to save the caller a fetch it already has to do anyway, (b) create an inconsistent contract where some validator methods take pre-loaded data and one fetches internally, and (c) bake one specific operation's error union into the validator, which conflicts with "map validation into the operation's own error, once, at the calling operation's boundary" (this codebase's own stated rule, cited only for completeness — but the underlying technical objections (a) and (b) stand independent of that rule).
5. Just leaving the ~5-8 lines duplicated, or merely reformatting each copy into a shorter Result-composition chain without actually extracting anything — rejected, I want real deduplication, not a cosmetic rewrite.

## What I need from you

A single, decisive, concrete design — exact code, exact type name(s), exact member signature(s) — for how to genuinely deduplicate this specific shared logic across these two specific callers, given the above constraints and rejections. If you think one of the previously-rejected shapes is actually correct despite the stated objections, say so plainly and directly rebut the specific objection rather than just re-proposing it. I do not want another list of tradeoffs — pick one answer and defend it.
