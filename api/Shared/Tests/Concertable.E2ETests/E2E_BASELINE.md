# E2E Baseline

The contract for which scenarios are **expected** to pass vs fail. `./e2e.ps1 regress` reads this file to know which scenarios to run; PR review reads it to see what changed.

<!-- ============================================================
     EDITING RULES — the regress script parser depends on these

     ALL data the parser reads lives BELOW the
       <!-- BASELINE-DATA-START -->
     marker further down this file. Anything ABOVE the marker is
     ignored, including this comment.

     Each scenario section MUST be formatted as:

         ### <Suite> <status> (<N>)

         ```text
         scenario name 1
         scenario name 2
         ...
         ```

     - Suite      : "B2B" or "Customer"
     - status     : "passing" or "failing"
     - (<N>)      : MUST equal the line count inside the fenced block
                    parser throws if mismatched
     - Fence      : MUST be ```text (not ``` alone, not ```markdown)
     - One scenario per line. No bullets, no quotes, no leading/trailing whitespace
     - Scenario name MUST exactly match the Reqnroll DisplayName
       (= the Scenario line in the .feature file, no decorations)

     UPDATE FLOW
     When `./e2e.ps1 run` shows a scenario crossing the line (now
     passes that didn't, or now fails that did), move it between
     the passing and failing fenced blocks below AND update both
     (N) counts in the headings AND update the summary table.
     ============================================================ -->

## Summary

Last reconciled: 2026-05-30 / commit `f53f6fe8` + uncommitted B2B seeding simulator session.

| Suite | Total | Passing | Failing |
|---|---|---|---|
| B2B | 23 | 7 | 16 |
| Customer | 7 | 2 | 5 |
| **Total** | **30** | **9** | **21** |

All failing scenarios cluster on Stripe payment flows (3DS challenge, "new card" variants, declined-card variants). Not in scope for the current branch.

<!-- BASELINE-DATA-START -->

## B2B (23 total)

### B2B passing (7)

```text
New artist manager registers, signs in, creates their artist profile
New venue manager registers, signs in, creates their venue
Venue manager signs in via OIDC
Venue manager books artist on a door split
Venue manager books artist on a flat fee
Venue manager books artist on a versus deal
Artist pays hire fee upfront to book venue
```

### B2B failing (16)

```text
Venue manager books artist on a door split with a new card
Venue manager 3DS authentication fails on door split
Venue manager completes 3DS challenge on door split
Venue manager door split card registration is declined
Venue manager books artist on a flat fee with a new card
Venue manager 3DS authentication fails on flat fee
Venue manager completes 3DS challenge on flat fee
Venue manager flat fee attempt is declined
Venue manager books artist on a versus deal with a new card
Venue manager 3DS authentication fails on versus
Venue manager completes 3DS challenge on versus
Venue manager versus card registration is declined
Artist pays hire fee upfront with a new card
Artist 3DS authentication fails on venue hire
Artist completes 3DS challenge on venue hire
Artist venue hire attempt is declined
```

## Customer (7 total)

### Customer passing (2)

```text
New customer registers and signs in
Customer signs in via OIDC
```

### Customer failing (5)

```text
Customer 3DS authentication fails
Customer completes 3DS challenge
Customer purchases a ticket using a new card and views the QR code
Customer purchase is declined
Customer searches for concerts, purchases a ticket, and views the QR code
```
