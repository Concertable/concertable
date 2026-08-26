# Code review — Feature/shared-email-renderer

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it.

**Reviewed up to commit:** `6dd1786271c6314b8a01210f75dbcc93c044620b`  _(2026-08-16)_

> Range reviewed: `863e0c3a..6dd17862`. Net diff: the shared MJML `IEmailRenderer` + packages + test
> (the intermediate `QuestPdfRenderer` rename and `CODE_CONVENTIONS` addition were reverted and cancel out).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No issues found. Checked correctness (Scriban binds the content, Mjml.Net compiles MJML → Outlook-safe
HTML; 2 tests prove binding, conditionals, and `<mjml>`/`{{ }}` fully resolved), microservice isolation
(shared lib, third-party deps only, no cross-service coupling), module boundaries (n/a — shared),
seeding (n/a), C# conventions (`internal sealed` impl + public `IEmailRenderer`/`IEmailContent`/
`RenderedEmail` in `.Application`, singleton registration, generic-named per the settled design), and
test coverage of the renderer's behaviour.

## Carry-forward to the #582 rewire (not a defect in this diff)

Scriban does not auto-HTML-escape. This diff ships no templates, so there is nothing unsafe here — but
the booking-confirmation `.mjml` added in #582 binds tenant-supplied legal names/addresses, so it MUST
`| html.escape` those values to preserve the injection-safety the removed `BookingConfirmationEmailGenerator`
provided via `WebUtility.HtmlEncode`. Enforce in #582 + cover with a test that binds `<`/`&`.
