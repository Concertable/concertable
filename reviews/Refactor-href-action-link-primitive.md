# Code review — Refactor/href-action-link-primitive

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `in-progress`
**Reviewed up to commit:** `b4496a9f8c61de683b8f8ef3c56bab31220464be`  `(2026-09-02)`
**Judgment:** `changes-requested`

## Review pass — 2026-09-02 — full

**Candidate base:** `3c993d9e4b1bc5898bb7f79297ae51da1fbfa005`
**Candidate head:** `2e3d6d391cbd4a3df75be015c34d8ed90c39379f`
**Candidate branch:** `Refactor/href-action-link-primitive`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:4bb37f54e973269c481bd453351f6347801acc8ced055dc04792eaf7e90ecf78` `(5 paths)`
**Candidate bundle:** `C:/Users/TOMMYS~1/AppData/Local/Temp/claude/C--Users-TommySeery-source-repos-Concertable/62ef6cf9-f6fe-43dd-a8f6-24f2683961a7/scratchpad/review-bundle-href`
**Candidate bundle identity:** `sha256:09bbadb793049f1756615ae8eb59d0d8f90515ba0108fd6d952f2fb7a2a65ce1`
**Work-order path:** `reviews/Refactor-href-action-link-primitive.md`
**Work-order mode:** `new`
**Pass judgment:** `changes-requested`

Layers run: native/general; correctness lens; changed-behaviour test-impact lens against
`standards/dotnet/testing/UNIT.md`. Rules routed from the frozen tree: `docs-and-debt`, `http-api`,
`result-terminals`, `microservice-boundaries`, `unit-testing` (generic and Concertable-side pairs).
No frozen path matches the merge gate's generic or repository `security_paths`, so no security layer
ran and no security marker is stamped.

Every accept/reject claim below was reproduced by the parent by executing `Href.TryFrom` and
`JsonSerializer.Deserialize` against this candidate, not taken from a lens on trust.

### Findings

- [x] **F1 — HIGH — correctness/boundary** — `api/Concertable.Shared/src/Concertable.Kernel/ValueObjects/Href.cs:21`
  The protocol-relative guard tests only a literal `"//"`, so one substituted character defeats it.
  Verified accepted by `Href.TryFrom`: a slash followed by a backslash then `evil.com`; a slash
  followed by TAB then `/evil.com`; the CR and LF variants of the same; and a path containing U+0001.
  A browser resolving the backslash form against the app origin parses the backslash as a slash per
  WHATWG, and strips TAB/CR/LF before parsing, so each of these resolves to a scheme-relative URL at
  `evil.com` — the exact cross-origin escape this check exists to prevent, emitted into a HATEOAS
  link the SPA re-issues. `NormalizeInput` trims only the ends, so interior control characters
  survive. Fix: before the leading-slash checks, reject any value containing a backslash or a
  character matching `char.IsControl`; then apply the same local-URL rule ASP.NET Core's
  `UrlHelperBase.IsLocalUrl` uses — first character a slash, and either length 1 or a second
  character that is neither a slash nor a backslash.

- [x] **F2 — MEDIUM — correctness** — `api/Concertable.Shared/src/Concertable.Kernel/ValueObjects/Href.cs:24`
  Splitting on slash and testing for a literal `..` segment is wrong in both directions.
  Under-inclusive: verified accepted are `/api/%2e%2e/admin`, `/api/%2E%2E/admin`, and the
  backslash-separated `..` form, each a parent traversal once decoded or once the backslash is read
  as a separator. Over-inclusive: it splits the query and fragment too, so a legitimate
  `/api/files?path=/a/../b` or a fragment ending in `..` is rejected with "must not traverse its
  parent" though its path is clean — a `DomainException` thrown while building a response. Fix: cut
  query and fragment first, `Uri.UnescapeDataString` the remainder, then split on both slash and
  backslash before testing for the `..` segment.

- [x] **F3 — MEDIUM — correctness** — `api/Concertable.Shared/src/Concertable.Kernel/ValueObjects/Href.cs:27`
  The terminating `Uri.TryCreate(trimmed, UriKind.Relative, out _)` validates nothing. Of seventeen
  hostile inputs run against it — including control characters, a raw space, and the `[`, `^` and `|`
  characters — it rejected none, and it is only reachable by a value that already starts with a
  slash. Its `Validation.Invalid` outcome is therefore dead, and it reads as a character check that
  is in fact absent, which is how F1 and F2 slipped past. Fix: delete the `Uri.TryCreate` branch and
  put the explicit character rule from F1 in its place, so the rejected set is stated rather than
  delegated to `System.Uri` leniency.
  Folded into the same fix: `var trimmed = value.Trim()` on line 16 is a no-op, because Vogen runs
  `NormalizeInput` before `Validate` and the value is already trimmed.

  **Disposition (F1-F3, one rewritten validator):** `Validate` now runs four ordered checks - required;
  no backslash, space or `char.IsControl` character anywhere; root-relative by the `IsLocalUrl` rule
  (first character `/`, and either length 1 or a second character that is not `/`); then `..` as a
  decoded segment of the path only, after `?` and `#` are cut. Rejecting the backslash outright makes
  F2's suggested `Split('/', '\')` unnecessary, so the split stays on `/`. The dead `Uri.TryCreate`
  branch and the redundant `.Trim()` are gone. Single-decode is the stated guarantee: it matches the
  one decode a server does, so `%252e%252e` is deliberately not treated as traversal.
  Evidence: `HrefTests` now covers the five cross-origin forms, the control character and the raw
  space, the two encoded-traversal forms, and the two previously over-rejected values
  (`/api/files?path=/a/../b`, `/api/concert/7#notes/..`) as accepted.
  `Concertable.Kernel.UnitTests` 271/271.

- [x] **F4 — HIGH — correctness** — `api/Concertable.Shared/src/Concertable.Shared.Api/Http/ActionLink.cs:8`
  `ActionLink` serializes but cannot be deserialized. Its only constructor is `private`, its
  properties are get-only, and it carries no `[JsonConstructor]`; deserializing it under the
  application's own serializer options throws, verified verbatim: `NotSupportedException:
  Deserialization of types without a parameterless constructor, a singular parameterized
  constructor, or a parameterized constructor annotated with 'JsonConstructorAttribute' is not
  supported. Type 'Concertable.Shared.Api.Http.ActionLink'.` This matters because the type's declared
  purpose is to replace four positional `internal sealed record ActionLink(string Href, string
  Method)` declarations, which *are* deserializable, and integration tests bind the real response
  types that carry them — `ApplicationWithdrawRejectApiTests.cs:193,205` calls
  `ReadAsync<ApplicationResponse<VenueApplicationActions>>()`. The migration would fail at runtime,
  and `ActionLinkWireFormatTests` pins the serialize direction only, so nothing here catches it.
  Fix: annotate the constructor `[JsonConstructor]` — its `href` and `method` parameter names bind to
  the camelCased properties — and add a round-trip assertion to `ActionLinkWireFormatTests`.

  **Disposition:** `[JsonConstructor]` on the constructor, which System.Text.Json honours while the
  constructor stays `private`, so factory-only construction is preserved rather than traded away.
  Evidence: two new round-trip tests — `ActionLink` alone, and a record envelope carrying two
  nullable `ActionLink` members, which is the shape the four module response types use. Both assert
  equality against the original, so the serialize and deserialize directions are now both pinned.

- [x] **F5 — LOW — docs-and-debt** — `api/Concertable.B2B/TECH_DEBT.md:149`
  The new debt entry cites code that does not exist on this candidate. There is no Api-layer
  `ApplicationMappers`: the Api-layer file is `Concert.Api/Mappers/ApplicationMapper.cs` (singular),
  and the only `ApplicationMappers.cs` sits in `Concert.Application/Mappers/`, which builds no action
  links. The frontend regex is cited at `app/web/shared/src/lib/actionLinkApi.ts`, which is absent
  here — on this branch it is `app/web/b2b/shared/src/features/concerts/api/actionLinkApi.ts`. A
  durable doc pointing at paths that do not exist is the dangling citation `docs-and-debt` forbids.
  Fix: correct both to the paths present on this branch.

  **Disposition:** corrected. The mapper list is now `ApplicationMapper`, `ConcertMappers`,
  `SelfBillingAgreementMappers` and `OpportunityMapper` under `Concert.Api/Mappers/`, plus
  `Conversations.Api/Mappers/MessageMappers`; the frontend reference is `apiPath` in
  `app/web/b2b/shared/src/features/concerts/api/actionLinkApi.ts`. All six cited paths were checked
  to exist on this branch.

- [x] **F6 — LOW — unit-testing** — `api/Concertable.Shared/tests/Concertable.Shared.Api.UnitTests/ActionLinkWireFormatTests.cs:13`
  `ApplicationOptions()` is a per-test factory method rebuilding a whole `ServiceCollection` and
  `ServiceProvider` on every call, for a value identical across every test. `UNIT.md` rejects this by
  name: "Never a per-test `CreateSut()`/`CreateService()` factory method. A private method rebuilt on
  every call is the constructor's job wearing a disguise." Fix: a `private readonly
  JsonSerializerOptions` field built in the test constructor and referenced with `this.`.
  Note for whoever fixes it: the unchanged `GenreWireFormatTests` in the same project has the same
  shape, so fixing only the new file leaves the two inconsistent. Converting both is preferable, but
  the neighbour is outside this candidate.

  **Disposition:** replaced by a `private readonly JsonSerializerOptions options` built in the test
  constructor and read as `this.options`. `GenreWireFormatTests` is unchanged and still carries the
  old shape — it is outside this candidate's path set, so it is not silently reworked here.

- [x] **F7 — LOW — unit-testing** — `api/Concertable.Shared/tests/Concertable.Shared.Api.UnitTests/ActionLinkWireFormatTests.cs:41`
  `Factory_RejectsAnHrefThatIsNotRootRelative` breaks `Method_Scenario_ExpectedBehaviour`: "Factory"
  is not a method on `ActionLink`, and the test exercises `Post` only. Fix: rename to
  `Post_HrefNotRootRelative_ThrowsDomainException`.

  **Disposition:** renamed to `Post_HrefNotRootRelative_ThrowsDomainException`, and a sibling
  `Get_HrefNotRootRelative_ThrowsDomainException` added so the rejection is asserted for both
  surviving factories rather than inferred from a shared code path.

- [x] **F8 — LOW — reuse** — `api/Concertable.Shared/src/Concertable.Shared.Api/Http/ActionLink.cs:22`
  `Put` and `Delete` have no caller and no test, and none of the `ActionLink` declarations this
  primitive replaces uses either verb — every existing one is `HttpMethods.Get` or `HttpMethods.Post`.
  Fix: delete both until a caller exists, and add them back with the caller that needs them.

  **Disposition:** both deleted. `Get` and `Post` are the only verbs any existing `ActionLink` uses.

Considered and deliberately not retained: no maximum length on `Href` (no backing column exists, so
the failure mode is speculative); the Vogen EF value converter being untested (nothing persists an
`Href`, and it mirrors `EmailAddress`); exception-message text not asserted, the grouped
invalid-input theory, and `TryFrom` not being driven through every `Validate` branch (all match the
established `EmailAddressTests` convention and share one code path); and `AddControllers()` inside a
unit test (the pre-existing, unchanged `GenreWireFormatTests` establishes it locally).

## Review pass — 2026-09-02 — incremental

**Candidate base:** `2e3d6d391cbd4a3df75be015c34d8ed90c39379f`
**Candidate head:** `b4496a9f8c61de683b8f8ef3c56bab31220464be`
**Candidate branch:** `Refactor/href-action-link-primitive`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:e244b109230d554e417c8461f1ba0a95432565a3d305e2dee6f538b925d8e690` `(6 paths)`
**Candidate bundle:** `C:/Users/TOMMYS~1/AppData/Local/Temp/claude/C--Users-TommySeery-source-repos-Concertable/62ef6cf9-f6fe-43dd-a8f6-24f2683961a7/scratchpad/review-bundle-href-inc`
**Candidate bundle identity:** `sha256:8fb5e615f9c2087c6c9b079a3aa8e923b25ffeb3f97983ec6dbcac7650b1c8dd`
**Work-order path:** `reviews/Refactor-href-action-link-primitive.md`
**Work-order mode:** `append`
**Pass judgment:** `changes-requested`

### Findings

Layers run: native/general; an adversarial correctness lens tasked only with refuting the claim that
`Href` now accepts nothing off-origin. Both reported, both were re-verified by the parent by executing
`Href.TryFrom` and `JsonSerializer.Deserialize`, and by reading the installed axios source. No frozen
path matches `security_paths`, so no security layer ran.

- [x] **F9 — HIGH — correctness/boundary** — `api/Concertable.Shared/src/Concertable.Kernel/ValueObjects/Href.cs:22`
  The root-relative rule inspected `value[1]` only, so an empty segment further along was accepted:
  `Href.TryFrom("/api//evil.com/x")` returned true. That value is not inert. `apiPath`
  (`app/web/b2b/shared/src/features/concerts/api/actionLinkApi.ts:4`) strips a leading `/api`, leaving
  `//evil.com/x`; axios `isAbsoluteURL` documents `//` as absolute
  (`app/node_modules/axios/lib/helpers/isAbsoluteURL.js`), and `buildFullPath` then returns the URL
  "untouched", dropping `baseURL`; `attachAuth` (`app/shared/src/lib/client.ts`) sets
  `Authorization: Bearer <token>` on every request unconditionally. So the request, and the caller's
  access token, leave for the attacker's host. `/api///evil.com` behaves the same, and `apiPath`'s
  regex is case-insensitive so `/apI//evil.com` does too.
  **Disposition:** the path is now rejected for containing `//` anywhere, not just at index 1, which
  closes the family rather than one index. Covered by `From_EmptyPathSegment_ThrowsDomainException`
  (`//host`, `/api//host`, `/api///host`) and by `TryFrom_InvalidHref_ReturnsFalse`.
  Not reachable before this branch: all existing `ActionLink` literals interpolate an int id.

- [x] **F10 — MEDIUM — correctness** — `api/Concertable.Shared/src/Concertable.Kernel/ValueObjects/Href.cs:16`
  Decoding happened in one branch and fed only the `..` test, so the structural rules saw raw bytes
  only. Verified accepted: `/%2Fevil.com` and `/%2fevil.com` (decode to `//evil.com`),
  `/%5Cevil.com` (to `/\evil.com`), and `/%09/`, `/%0D/`, `/%0A/` (to the tab, CR and LF forms) —
  the percent-encoded twins of the four raw payloads the tests already asserted must throw. Accepting
  a value while rejecting its raw equivalent is incoherent whatever a given client does with it.
  **Disposition:** `Validate` now applies one `PathFault` rule set to both the raw path and its
  once-decoded form, so every raw rejection has an encoded counterpart. Single decode remains the
  stated guarantee, so a double-encoded form is still not traversal. Each encoded form above is now
  an asserted case.

- [x] **F11 — MEDIUM — correctness** — `api/Concertable.Shared/src/Concertable.Shared.Api/Http/ActionLink.cs:9`
  `[JsonConstructor]` restored the round trip and simultaneously opened an absent-member hole:
  `Deserialize<ActionLink>` of `{"method":"POST"}` succeeded and produced an `ActionLink` whose
  non-nullable `Href` was null, so `link.Href.Value` threw `NullReferenceException`. `Href` *content*
  was never bypassable — Vogen's converter runs `Validate` — but absence was.
  **Disposition:** the review's suggested `[JsonRequired]` does not work here; System.Text.Json
  rejects it outright on a get-only property (`JsonPropertyInfo 'href' ... is marked required but does
  not specify a setter`), which broke serialization before it could help. The constructor now guards
  with `DomainException.ThrowIfNull` and `ThrowIfNullOrWhiteSpace`, the helpers `Concertable.Kernel`
  already provides, which cover the deserialization path and every other. Pinned by
  `Deserialize_MemberMissing_Throws` and `Deserialize_HrefThatWouldLeaveTheOrigin_Throws`.

- [x] **F12 — MEDIUM — correctness** — `api/Concertable.Shared/src/Concertable.Kernel/ValueObjects/Href.cs:16`
  The character denylist scanned the whole value while the traversal check correctly scoped itself to
  the path, so `/api/report?filename=C:\temp\out.csv` was rejected — a `DomainException` thrown
  while building a response over a backslash that WHATWG only treats as a separator in path state, not
  query state.
  **Disposition:** the backslash and empty-segment rules are scoped to `value.Split('?', '#')[0]`. The
  raw-space rule is dropped entirely rather than rescoped: an unencoded space is a caller formatting
  bug, not a boundary threat, and rejecting it bought nothing. Control characters stay rejected across
  the whole value, because a browser strips tab/CR/LF from anywhere in a URL and can reshape it.
  `From_QueryOrFragmentContent_IsNotJudgedAsAPath` now asserts the backslash-in-query and
  space-in-query cases are accepted.

- [x] **F13 — LOW — unit-testing** — `api/Concertable.Shared/tests/Concertable.Kernel.UnitTests/ValueObjects/HrefTests.cs:30`
  Two theory names described rules their data never reached: `""` and `"   "` sat under
  `From_NotRootRelative_...` though the required-value check rejects them first, and four of the five
  `From_ValueThatResolvesCrossOrigin_...` inputs were caught by the character check, leaving one input
  actually exercising the root-relative rule.
  **Disposition:** split by the rule each input reaches — `From_MissingValue_`,
  `From_NotRootRelative_`, `From_EmptyPathSegment_`, `From_Backslash_`, `From_ControlCharacter_` and
  `From_TraversingPath_`, each holding only inputs that reach the check it names.

Fixed in the follow-up commit; a third pass covers that delta.
