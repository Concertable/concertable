# Result pattern — Reunion packaging and migration state

The rules themselves live in skills: `result-carriers` (choosing and composing `Result`/`Option`),
`result-errors` (typed `XError` unions, definitions, published codes), `result-terminals` (HTTP, gRPC,
worker and exception terminals), `validation` (`ValidationResult` and validator choice), and `proto` (what
may cross the wire). They apply to every backend service here; existing code using another carrier or an
older construction style is migration debt, not precedent.

This file carries only what is specific to *this* repo.

## Reference the Reunion package whose API the project uses, at the service's own pin

| Package | Owns |
|---|---|
| `Reunion` | `Result`, `Result<TValue>`, `Result<TValue, TError>`, `UnitResult<TError>`, `Option<T>`, their named cases, composition, collection and task extensions |
| `Reunion.Errors` | `IError`, `ErrorDefinition`, `ErrorKind`, `ValidationErrors`, `ErrorCodeAttribute`, definition factories |
| `Reunion.Validation` | `ValidationResult`, its `Valid`/`Invalid` cases, validation accumulation |
| `Reunion.AspNetCore` | Minimal API and MVC terminal adapters |

Each service owns its exact versions in its service-local `Directory.Packages.props`. Keep every Reunion
package in a service on **one** version; the current baseline is `0.1.0-alpha.8`. Reference packages
directly rather than relying on a transitive dependency.

## Never redistribute Reunion through a Concertable package

Not through `Concertable.Kernel`, not `Concertable.Shared.Api`, not any other. A service references
Reunion itself.

The legacy `Concertable.Kernel.Functional` carriers and `Concertable.Shared.Api.Results` terminals survive
only until their owning migrations remove them; new and changed contracts use Reunion directly. Architecture
tests (`ReunionArchitectureTests`, `TypedResultArchitectureTests`) fail a build that reintroduces either.

## The reusable gRPC cancellation predicate lives in `Concertable.Grpc`

Along with operation-detail extraction. The concrete error-case map and the contract-mismatch exception
stay in the owning client — see the `proto` and `result-terminals` skills.

## A Result-based change is verified against the standalone package closure

Beyond the branch/definition/terminal coverage the skills require, a changed operation also owes: exact
package versions with no mixed Reunion graph, and a service-carve restore and build for every changed
package closure. Build and test the service against that closure, not only the monorepo source graph. A
published contract change follows the repository's publish-and-sync cut-over process.
