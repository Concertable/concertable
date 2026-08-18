# Audit findings — guidance-docs restructure

Two independent read-only audits, 2026-08-18, briefed on the confirmed paraphrase-loss template
(`WebApplicationFactory` → "host factory"). **16 defects.** Delete this file once all are closed.

**Read the plan's "target structure — four tiers" section first.** Several findings move depending on which
tier a doc ends up in, so the tier split (create `react-agents`; add `dotnet/` + `react/` to
`agent-standards`) comes before fixing these.

## P0 — structural: the process corpus was COPIED, not moved

`standards/process/` (7 docs, 423 lines) was extracted and its Concertable originals were left at **full
length** with **nothing linking them**. Verified mechanically: searching all Concertable markdown for
`` `merging` ``, `` `git-branching` ``, `` `committing` ``, `` `remote-validation` ``, `` `failing-tests` ``,
`` `docs-and-debt` ``, `` `plans` `` returns **zero hits** — the extracted corpus is referenced from nowhere
in the consuming repo. `MERGING.md` carries a near-byte-for-byte copy of root `AGENTS.md`'s 25-line poll loop
(same 8 marker lines: `cleanpolls`, `mergeQueueEntry`, `mgfail`), differing only in `main`→`base` wording.

This is the exact disease the restructure exists to cure, at full scale. The React half was done correctly
(`app/agents/*` slimmed to inventory; `app/AGENTS.md` and `docs/INDEX.md` name the skills). The process half
is extracted but not cut over. Same pattern for `COMMITTING.md`/`PLANS.md` vs `plans/AGENTS.md`,
`BRANCHING.md` vs root `AGENTS.md` "Git branch", `REMOTE_VALIDATION.md` vs `docs/REMOTE_VALIDATION.md`.

## P0 — correctness: following the doc now produces a violation

1. **`dotnet/STYLE.md` dropped the `[LoggerMessage]` carve-out**, so the `extension()` rule reads absolute —
   while `LOGGING.md`'s own canonical example is
   `internal static partial void PublishedOrderEvents(this ILogger logger, int count)`, the shape the rule
   now bans. The corpus contradicts itself inside one repo. Also lost: the "touch a container, migrate every
   ordinary member in it" clause — the mechanism preventing a class mixing both forms. Only "don't add new
   ones" survived, which permits exactly the mixed container the original banned.
2. **The `XMappers` worked example was rewritten into the banned legacy form**, in `NAMING.md` *and*
   `structure/PROTO.md` (`public static Shipment ToShipment(this ShipmentEntity entity)`). The original used
   `extension(Receiver)` blocks and was the only statement of how the two rules combine. An agent copying the
   canonical example now writes a violation.
3. **`data/PERSISTENCE.md` teaches a signature the source says does not exist** —
   `Repository<TEntity, OrderDbContext, Guid>`, threading the concrete context as a generic argument, where
   the source states "the shared bases deliberately have no concrete `TContext` parameter". The three-row
   capability triple (`IReadDbContext` → `IReadRepository` → `ReadRepository`, etc.) that decided *which*
   base to inherit is absent from the standards. Concertable's thinned doc still states it correctly, so the
   generic doc is the wrong one.

## P1 — paraphrase-losses (identifier deleted, prose left looking complete)

| # | Doc | Lost | Why it matters |
|---|---|---|---|
| 4 | `react/HTTP.md`, `react/STACK.md` | **`axios` entirely** — `axios.create()`, `AxiosResponse<T>`, `{ responseType: "arraybuffer" }`. Zero hits for `axios` in the whole standards tree | `create()` is not greppable or callable. `STACK.md` names a library in every row except HTTP, so "one library per job" has no incumbent, and its own "no second HTTP client" ban never names the first |
| 5 | `dotnet/testing/E2E.md` | **`Reqnroll`, `Playwright`** — zero hits in either repo | Step-binding and headless rules are meaningless without naming the binding framework and the driver |
| 6 | `dotnet/structure/SERVICE_BOUNDARIES.md` | **`Aspire`, `AddServiceDiscovery()`, `AddServiceDefaults()`, `AddStandardResilienceHandler()`, `AddGrpcClient<T>()`, `OpenTelemetry`** — plus the whole scope-limiting negative ("Aspire does *not* generate `.proto`, share contracts, version, or handle auth") | The negative existed precisely because that is the assumption people make. Nothing replaces it. "Apply telemetry, health checks and a resilience handler uniformly" also loses the original point that *one* call does all three |
| 7 | `process/REMOTE_VALIDATION.md` | **`Docker` entirely** (`docker ps`→"`ps`", `docker run hello-world`→"a `hello-world` run", `docker-proxy`→"the host-side proxy") and the diagnostic string **`pre-login handshake`** | The error text is the entire recognition mechanism — a doc that says "the signature is X" then withholds X |
| 8 | `process/MERGING.md` | **`Monitor`** → "a detached watcher tool" | The rule exists solely to stop one specific tool call; a reader cannot tell which tool is banned |
| 9 | `dotnet/NAMING.md` | The suffix table's entire **Precedent column** — `StringBuilder`, `UriBuilder`, `WebApplicationBuilder`, `TimeProvider`, `IFileProvider`, `IHttpContextAccessor`, `LinkGenerator`, `RandomNumberGenerator`, `IHttpClientFactory`, `ILoggerFactory`, `IUserStore`, `WebUtility`/`HttpUtility` | These were the calibration anchors for a distinction the doc itself calls "mechanics, not vibes" — `StringBuilder` vs `RandomNumberGenerator` vs `IHttpClientFactory`. `IUrlHelper` survived in the prose below, showing the removal was mechanical rather than judged |
| 10 | `dotnet/testing/INTEGRATION.md` | **`[Collection]`, `InitializeAsync()`** | "The test's initialization step" does not say which hook, and constructor-vs-`InitializeAsync` is a real silent failure — the reset runs at the wrong time |
| 11 | `dotnet/testing/INTEGRATION.md` | **`Environments`, `IHostEnvironment`** | "The framework's own environment types" does not say what to extend, which is the entire content of the rule |
| 12 | `react/STRUCTURE.md`, `react/SERVER_STATE.md` | **`.data`, `.isPending`, `.mutate`** and the litmus built on them | The members *were* the rule: they decide whether a hook must carry the mandatory `…Query`/`…Mutation` suffix. "Returns the library's result verbatim" is unfalsifiable at review |
| 13 | `react/SERVER_STATE.md`, `react/HTTP.md` | **`useQuery`, `useMutation`, `QueryCache`, `MutationCache`, `mutateAsync`** | All five are greppable in a diff; "an awaited mutation" is not |
| 14 | `react/HTTP.md` | **`silenceErrors`**, and `expectedErrors` demoted into a code sample | The mechanism survived, but "silence errors entirely" does not tell anyone what to type |
| 15 | `react/HTTP.md` | The retry cap **`2`** → "capped" | The status literals survived; the one number a reviewer checks a `retry:` value against became an adjective |
| 16 | `process/PLANS.md` | **`grep -rniE`** and the explicit casing enumeration (PascalCase, camelCase, snake, kebab) | The rule's stated point is "remove the discretion", yet the runnable command became a description of itself |

## P2 — gaps and duplication

- **"One repository per entity" has no generic home.** Only in Concertable's `api/agents/CODE_PATTERNS.md`.
  Headline and detection heuristic are fully generic ("its interface mixes queries for two or more unrelated
  entity types"); only the counts are product roster.
- **`InsertAsync` vs `AddAsync` + `SaveChangesAsync`** survives only in the product doc; the underlying rule
  ("stage-and-save in one call when nothing else is staged") is generic.
- **`FAILING_TESTS.md` lost the routing concept** — the original routed per tier to a named debug skill. The
  names are repo-local, but the *concept* of routing left with them.
- **`isAxiosError` is stated three times** — `app/web/AGENTS.md` (owner), `react/HTTP.md` (generic), and
  `app/agents/CODE_CONVENTIONS.md`, which restates the rule *and* links its owner. Keep the inventory half
  (`isApiError` lives at `@concertable/shared/lib/apiError`); the ban clause is the duplicate.

## Already fixed this session

- `testing/UNIT.md` / `INTEGRATION.md`: `WebApplicationFactory`, `Testcontainers`, `Respawn` restored, with
  the meta-note on why the names matter; per-tier assertion assignment restored; the Shouldly open call
  carried across **as open**.
- The `Concertable.Testing.Integration` import-then-restate double-write, deduped.

## Cleared — do not re-audit

Results carriers/errors/terminals, proto, keyed strategies, modules, HTTP API, comments, seeding (both
sanctioned exceptions and the dependency-direction paragraph), multitenancy (the `RS0030` chain intact),
validation (both invocation rows verbatim), unit-test shape, `IScoped<T>` rules, zod/`z.infer`/`safeParse`
and the bang-is-missing-validation argument, Zustand mechanics, `useEffect` carve-outs, `$type` unions.
The generated plugin copy is honest delivery — `diff -r` clean against source, CI-checked. Two apparent
reversals chased and cleared as deliberate updates: controllers `public`→`internal`, shared reference data
`SharedDbContext`→enum.
