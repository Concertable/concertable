# Investigation: are our tenant roles complete vs GigPig (and peers)?

## What I need from you

Research **GigPig** (gigpig.uk — UK live-music booking platform connecting venues and
artists/musicians) and **2–3 comparable platforms** (e.g. other venue/act management or
gig-booking SaaS — pick the closest real competitors you can find). Compare how they model
**team / staff roles and permissions** against the role set I already have (below).

Then answer one question: **do I need to add any more roles, or is my current set enough to
behave exactly like GigPig does?**

**Bias hard toward minimal.** I do not want a padded, theoretically-complete RBAC model. Only
recommend a new role if it is genuinely required to match how these platforms actually behave —
i.e. there's a real job a real venue/artist team member does that *none* of my existing roles can
be cleanly mapped to. If my six roles already cover it (even if the name differs), say so plainly
and recommend **no change**.

## Important — scope

- **The roles are NOT implemented or enforced in code yet.** That's the *next* PR. This
  investigation is purely about whether the **set of roles is complete** — do not comment on
  implementation, enforcement, code structure, or the permission-checking mechanism.
- This is the **final investigation before merge.** If your verdict is "the six roles are
  sufficient," I merge as-is. If you find a genuine, well-justified gap, I add the role(s) first.
- Don't redesign the permission taxonomy. Take the permissions as given; only reason about whether
  the **roles** (the named bundles) cover the real-world team structure.

## What I currently have

Two **personas** (a tenant is one or the other, fixed at signup):

- **Venue** — a venue posts opportunities, reviews applications, runs shows.
- **Artist** — an artist/act applies to opportunities, plays shows.

Six **roles** (exactly one per member; every tenant always has at least one Owner):

| Role | Intent |
|------|--------|
| **Owner** | Full control of the tenant, incl. billing, members, deletion |
| **Manager** | Runs day-to-day operations + bookings; can invite members; no finance/payout control |
| **Finance** | Money only — payouts, settlements; no booking/ops control |
| **Staff** | General operational helper; messaging + day-of-show ops |
| **Door** | Front-of-house / check-in only |
| **Sound** | Sound engineer — day-of-show ops only |

### Permission matrix (what each role can do)

Permissions are either **shared** (both personas), **(V)** venue-only, or **(A)** artist-only.
Two permissions are **reserved** — defined now but only enforced once "day-of-show" ships
(`concerts.ops_edit`, `concerts.check_in`).

**Shared permissions by role:**

| Permission | Owner | Manager | Finance | Staff | Door | Sound |
|---|:-:|:-:|:-:|:-:|:-:|:-:|
| operations.view | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| profile.edit | ✓ | ✓ | | | | |
| payouts.manage | ✓ | | ✓ | | | |
| settlement.view | ✓ | ✓ | ✓ | | | |
| settlement.trigger | ✓ | | ✓ | | | |
| tenant.settings.edit | ✓ | | | | | |
| tenant.delete | ✓ | | | | | |
| members.invite | ✓ | ✓ | | | | |
| members.remove | ✓ | | | | | |
| members.manage_roles | ✓ | | | | | |
| messages.read | ✓ | ✓ | ✓ | ✓ | | |
| messages.send | ✓ | ✓ | | ✓ | | |
| concerts.ops_edit *(reserved)* | ✓ | ✓ | | ✓ | | ✓ |

**Venue-only (V) permissions by role:**

| Permission | Owner | Manager | Staff | Door |
|---|:-:|:-:|:-:|:-:|
| opportunities.manage | ✓ | ✓ | | |
| applications.decide | ✓ | ✓ | | |
| concerts.manage | ✓ | ✓ | | |
| concerts.check_in *(reserved)* | ✓ | ✓ | ✓ | ✓ |

**Artist-only (A) permissions by role:**

| Permission | Owner | Manager |
|---|:-:|:-:|
| applications.submit | ✓ | ✓ |

(Finance / Sound hold no persona-exclusive permissions — they're money- and
day-of-show-scoped respectively.)

## Deliverable

A short report with:

1. **Verdict** up front — one line: *"Six roles are sufficient — merge"* **or** *"Add N role(s)."*
2. **Per-platform comparison** — for GigPig and each peer, what roles/permission tiers they expose,
   with a source/link where possible. Call out where their model differs from mine.
3. **Gap analysis** — for any role you'd recommend adding: the real-world job it represents, why
   none of my six cover it, and the rough permission bundle it would hold. If there's no gap, say
   so and stop — don't invent one.
4. **Anything I have that they don't** — if I'm carrying a role that has no real-world counterpart
   (over-modelling), flag it so I can consider cutting it.

Keep it concise and evidence-led. Minimalism is the goal: the best outcome is "you already have
enough."
