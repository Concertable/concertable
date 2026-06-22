# How to run a good `/deep-research` discovery

> A reusable guide + fill-in template. Copy the template at the bottom, fill the brackets, and paste it after `/deep-research`.

---

## What the tool is (and isn't)

`/deep-research` runs a 5-stage agent pipeline: **Scope → Search → Fetch → Verify → Synthesize.**
- It **fans out web searches**, fetches ~15–30 sources, extracts falsifiable claims, then has **3 independent skeptic agents vote on each claim** (a claim needs 2/3 to refute to be killed). The surviving claims are unusually trustworthy.
- It's **token-heavy** (~2–3M tokens, ~100+ agents). The cost buys *confidence*, not *volume* — you get a smaller set of well-verified facts, not a big pile of maybes.

**Use it for:** high-stakes questions that are genuinely answerable from the public web — market/competitor facts, API capabilities, regulations, pricing, "is X still true in 2026."

**Don't use it for:** questions that are mostly about *your own codebase* (it can't see your repo). For those, a codebase scout + a handful of targeted web fetches gets ~80% of the value for ~10% of the tokens. If a question is *part* codebase, *part* web (like the marketplace one), give the tool the codebase facts up front as **CONTEXT** so it can target the web half.

---

## The five things that made the last prompt work

A weak prompt is a topic ("research event ticketing APIs"). A strong prompt is **decision-shaped**. Include all five:

1. **CONTEXT** — what your product/situation is, in plain facts. The tool can't see your codebase, so spell out the relevant bits (what the thing does, how money flows, what's already built). 3–6 sentences.
2. **HARD CONSTRAINT** — the one fact that reframes everything, stated up front so every agent keeps it in view. *(Last time: "settlement is computed from ticket-sales revenue, so whoever holds the money matters.")* This is what stops the research from being a generic survey.
3. **THE DECISION** — what you're actually trying to decide. Research aimed at a decision is sharper than research aimed at a topic.
4. **NUMBERED RESEARCH QUESTIONS** — explicit, with the **named entities** to cover (specific platforms, specific competitors) and **what "answered" means** for each (e.g. "is it available *today* vs deprecated/gated", "who holds the money", "what does it cost"). Vague questions get vague answers.
5. **DELIVERABLE FRAMING** — one line on what the report should let you *do* ("let me decide whether to build X or delegate it").

Also specify **scope**: region (UK?), recency ("current, 2025–2026"), and "prefer primary sources."

---

## Mechanics & gotchas

- **Invoke:** type `/deep-research` then your prompt. If the question is underspecified it'll ask 2–3 clarifying questions first — answer them; they sharpen the search angles.
- **It runs in the background.** You'll be notified on completion; `/workflows` shows live progress.
- **If the final report is missing** ("synthesis skipped / failed — returning N claims unmerged"): that's almost always a transient `529 Overloaded` server error on the last step, **not** a bad run. The expensive stages already succeeded and are **cached**. Just say *"resume the deep-research synthesis"* — it re-runs only the failed step (seconds + pennies), not the 100+ agents. **Do not re-run the whole thing.**
- **Treat unverified extras honestly:** the report separates 3-vote-verified claims from softer backfill. Facts that only one source asserts (or that come from a vendor's own marketing) are directional, not gospel — the report should say which is which.

---

## Fill-in template

```
CONTEXT — what I'm analysing:
[2–6 plain sentences: what the product/situation is, how it works, what's already
built, and any numbers/mechanics the web researcher can't otherwise know.]

HARD CONSTRAINT to keep front-and-centre:
[The one fact that reframes the whole question. State it as a constraint the
answer must respect.]

THE DECISION I'm trying to make:
[The specific choice. e.g. "Build X ourselves vs delegate it to an incumbent via API."]

RESEARCH QUESTIONS (answer with current, cited facts; prefer primary sources; region = [UK/US/global]; recency = [2025–2026]):
1. [Question naming the specific entities/platforms/competitors to cover, and stating
   what counts as an answer — availability TODAY, cost, gating, who-holds-what, etc.]
2. [...]
3. [...]
4. [Competitive landscape: name the specific competitors; ask exactly what each does
   re: the thing that matters to your decision.]
5. [Strategy/regulatory angle if relevant: name the jurisdiction and the bodies/rules.]

DELIVERABLE framing:
[One line: what decision this report should let me make, and the 2–3 sub-questions
the conclusion must resolve.]
```

---

## Worked example (the prompt that produced the marketplace research)

The full prompt is in this repo's git history (the `/deep-research` invocation, 2026-06-22). Its shape:
- **CONTEXT:** "Concertable is a UK two-sided live-music SaaS; B2B side negotiates one of four contract types and a settlement engine auto-pays per contract via Stripe Connect; consumer side currently owns ticketing."
- **HARD CONSTRAINT:** "Door-split/versus settlements are computed FROM ticket-sales revenue — so whoever owns checkout must expose sales data back, or settlement can't run."
- **DECISION:** "Deploy B2B first; can we delegate event listing/ticketing to an external marketplace's API instead of building our own, and still run settlement?"
- **QUESTIONS:** named DICE, Skiddle, See Tickets, Fatsoma, Eventbrite, Ticketmaster, Universe, RA, Songkick, Bandsintown — and for each asked: create events via API today? read back sales/orders/attendees? who holds the money + payout timing? plus a GigPig/competitor question and a UK-payments-regulation question.
- **DELIVERABLE:** "Let me decide (a) is delegation viable given the sales-data constraint, (b) the leanest MVP that proves the USP, (c) the best long-term codebase vision."

That structure is why the output was decision-useful rather than a Wikipedia dump.
