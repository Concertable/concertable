# Semantic typed HTTP terminals

> Next steps live in @plans/typed-result/HTTP_RESULT_TERMINALS_PROGRESS.md → `## Next Steps`.

## Goal

Replace vague `Concertable.Shared.Api.Results` terminal names with names that state both HTTP
outcomes. Result failures terminate as ProblemDetails; Option absence terminates through an explicit
endpoint policy. Keep the Kernel HTTP-free.

## Naming rule

- Do not expose a generic terminal whose name omits its HTTP outcomes.
- Result terminals name their success and failure paths: `ToOkOrProblem`, `ToCreatedOrProblem`,
  `ToCreatedAtOrProblem`, and `ToNoContentOrProblem`.
- Option terminals name their Some and None paths: `ToOkOrNotFound` or `ToOkOrNoContent`.
- Read error and HTTP response names do not start with `Get`: use `VenueError` / `VenueResponse`, not
  `GetVenueError` / `GetVenueResponse`. Mutation errors retain their operation name, such as
  `CreateVenueError`.

`Option<T>` represents ordinary application absence only. If an application operation must report a
caller-actionable reason such as unauthenticated, forbidden, conflict, or validation failure, it
returns `Result<TValue, TError>` and an operation-owned typed error. Authentication/authorization
that is decided before an action remains at the ASP.NET authorization boundary.

## Package topology and delivery

`Concertable.Shared.Api` is a published platform package. Customer, B2B, Payment, and Search API
projects consume it through their per-service `ConcertablePlatformVersion` pins. Replacing its public
generic terminal API is therefore a breaking package change and cannot be consumed atomically.

The merged Reunion integration plan changes the same public package and must remain the sole producer
and generated-sync owner. This worktree therefore prepares, verifies, reviews, and commits the
semantic terminal implementation locally without pushing or opening a competing package PR. After
matching Reunion packages publish, the Reunion Phase 3 producer incorporates this checkpoint; its
Phase 4 generated sync migrates every consumer once. Customer PR #425 then reconciles against that
integrated baseline.

## Phases

### Phase 1 — Shared.Api semantic terminal checkpoint

- [x] Replace the current Result terminal API and tests with semantic `*OrProblem` names.
- [x] Add tested `Option<T>` HTTP terminals for each supported absence status without introducing MVC
  concerns into Kernel.
- [x] Record the read-name rule in `api/agents/CODE_CONVENTIONS.md`.
- [ ] Preserve the final local corrections, verify them, commit the complete checkpoint, and finish
  code review. Do not push or publish from this branch.

### Phase 2 — Reunion producer handoff and Customer consumer migration

- After Reunion Phase 2 publishes matching packages, incorporate the verified local terminal
  checkpoint into the one Reunion Phase 3 Shared producer PR.
- Let Reunion Phase 4 migrate the repository consumer surface and generated platform pin once.
- After that sync merges, update PR #425 to use `*OrProblem` and `*Or<NoneStatus>` terminals. Keep
  `Option` for ordinary absence, and change Customer's current-user lookup to a typed
  `Result<CustomerDto, CurrentUserError>` so its unauthenticated outcome reaches `ToOkOrProblem`.
- Remove every old terminal name and verify the affected Customer integration, Shared.Api, Release
  solution, and standalone package-closure gates.

## Definition of done

- No public generic terminal name that hides its HTTP outcomes remains.
- Each controller terminal makes every possible HTTP response path visible in its method name.
- Kernel remains transport-free; typed errors remain operation-owned and response suffixes stay HTTP-only.
- The local checkpoint is incorporated into the Reunion Shared producer, its generated platform sync,
  Customer consumer PR, and Customer's resulting platform sync, all terminal green.
