# Cookie / browser-storage consent — competitive + legal research

> Reference doc for the `BROWSER_STORAGE_CONSENT` plan. Compiled 2026-08-10 from three
> parallel research passes (UK gig marketplaces; ticketing platforms + CMPs; UK legal baseline).
> It exists to settle one decision: **keep the consent banner and make it a *real* gate, or adopt a
> CMP** — and to specify what "compliant + production-ready" means for Concertable's four SPAs.

## Why this doc exists

Phase 1's audit found Concertable's Analytics/Marketing banner gates nothing (`hasConsent`/
`onConsentChange` have zero consumers) and no analytics/marketing tech loads yet. The first decision
was "decorative → remove." That was **wrong**: analytics + marketing/advertising tracking is on the
commercial roadmap, so the banner is *infrastructure ahead of tech*, and under UK law it becomes
mandatory the moment that tech loads. This research confirms the direction and picks the shape.

## 1. Legal baseline (UK) — the decisive framing

**The law changed under us.** The **Data (Use and Access) Act 2025 (DUAA)** amended PECR **Regulation 6**
with effect **5 Feb 2026**, and the ICO reissued its guidance as *"Guidance on the use of storage and
access technologies"* (final Apr 2026). Post-DUAA position:

- **PECR reg 6 is the trigger** (not UK-GDPR): storing/reading *any* info on a device — cookies,
  `localStorage`, `sessionStorage`, `IndexedDB`, pixels, SDKs — needs **consent** to the UK-GDPR
  standard (freely given, specific, informed, unambiguous, affirmative action), unless exempt.
- **Exemptions are narrow:** communication-transmission, and **"strictly necessary"** = *essential to a
  service the user explicitly requested* (judged from the user's ask, not ours). DUAA adds new
  no-opt-in-but-must-offer-objection categories: **first-party** analytics *used solely by us to measure/
  improve the service*, appearance/functionality customisation, and emergency location. **Crucially the
  first-party-analytics exemption explicitly excludes anything shared with third parties or tied to
  advertising** — so **GA4 (shares data with Google) still requires consent.**
- **Enforcement is live and aimed at exactly our defect.** ICO wrote to the top-100 (Nov 2023) then
  top-1,000 (2025) UK sites on "Reject-all must be as easy as Accept-all"; by Dec 2025, 979/1,000
  compliant, **17 enforcement notices**. The **Sept-2024 Sky Betting reprimand** was for dropping ad
  cookies *before the user chose* — a gate-nothing banner, i.e. Concertable's current shape. DUAA raised
  the PECR fine ceiling from £500k to **£17.5m / 4% of global turnover**.
- **Google Consent Mode v2 is effectively mandatory** to run GA4/Google Ads for UK/EEA users: on every
  page, **before any Google tag**, set all four signals (`ad_storage`, `analytics_storage`,
  `ad_user_data`, `ad_personalization`) to `denied`; flip to `granted` on consent; re-evaluate on SPA
  navigation.

**Classification for Concertable's storage:**

| Item | Class | Consent? | Action |
|---|---|---|---|
| Auth / OIDC (`oidc.*`) | Strictly necessary | No | Disclose |
| Stripe (`__stripe_mid/sid`, Radar fraud) | Strictly necessary (payment/fraud on a checkout the user started) | **No — do not gate** | Disclose; **load lazily at checkout** (today it loads at boot, weakening the exemption) |
| Google Maps (`NID` etc.) | Functional / third-party (Google) | **Yes, except on pages where the map *is* the requested task** | Load lazily; gate under a **functional** category off its core-search pages |
| `theme` preference | Functional (no PII) | Borderline exempt | Disclose |
| Future **GA4** | Analytics (shares w/ Google) | **Yes — opt-in** | Gate under `analytics` + Consent Mode v2 |
| Future **ads / remarketing** (Google Ads, Meta) | Marketing | **Yes — opt-in** | Gate under `marketing` + Consent Mode v2 |
| `sidebar_state` cookie | Unused (write-only, never read) | n/a | **Removed** (Phase 2 — done) |

**A compliant banner must:** default non-essential **off**; **block matching scripts until consent**;
show **Reject-all at equal prominence to Accept-all** on the first layer; allow **per-category** choice;
offer an **always-reachable withdraw/re-open** control; keep a **timestamped consent record**.

## 2. What comparable platforms actually do

### Closest analogues — UK gig / artist-booking marketplaces

| Platform | Banner | CMP | Categories | Reject-all parity | Trackers |
|---|---|---|---|---|---|
| **GigXchange** | Yes | Custom + **GA4 Consent Mode v2** | Essential / Analytics (**off by default, opt-in**) | Effective (analytics off by default) + footer re-open | GA4 only |
| **GigPig** (closest peer) | Yes | Custom (OneTrust stock copy; vendor unconfirmed) | Necessary/Analytics/Marketing | **No equal-prominence Reject-all** (ICO gap) | GA, Crazy Egg, **Google Ads + Meta + X retargeting** |
| **gigmit** | Yes (policy) | WordPress consent plugin | opt-in, granular | Yes | ad IDs, many third parties |
| **Poptop** | policy-only | none | Necessary/Performance/Functionality/Targeting | implied-consent (below bar) | none named |
| Alive Network / Encore / Function Central | mostly browser-settings + implied consent (**below current UK bar**) | none | 3–4 informal | mostly no | Google Analytics |

Takeaways: modern careful peers (**GigXchange**, gigmit) = **opt-in, analytics-off-by-default, custom
banner + Consent Mode v2**. The market leader **GigPig** already runs full retargeting but its banner
lacks Reject-all parity — the easiest thing for Concertable to beat. Older agencies are non-compliant.

### Larger ticketing / concert platforms (incl. B2B/organiser portals)

| Cohort | CMP | Pattern |
|---|---|---|
| **Ticketmaster, Ticketmaster Business/TM1, Universe, TicketWeb** (Live Nation); **AXS** (AEG); **Songkick** (Warner) | **OneTrust** (confirmed via `OptanonConsent` / `#cookieSettingBtn`) | 4–5 categories (Strictly Necessary / Performance / Functional / Advertising / Social), category **+ vendor**-level control, explicit reject-all, persistent preference centre |
| **DICE, Skiddle, Resident Advisor, Ents24, WeGotTickets, Fatsoma** | **Custom** (no CMP) | Quality varies wildly — DICE granular per-category; **Fatsoma accept-only, no reject (worst)**; Ents24/WeGotTickets dated browser-level |
| **Eventbrite** | custom/unconfirmed | Real preference centre with a bespoke **"Organizer"** category — a precedent for a two-sided marketplace carving out B2B-context tracking |

**Headline finding — the split is by scale:** big, well-resourced incumbents standardise on **OneTrust**
(enterprise, ~low-5-figures/yr); mid-market/indie players **hand-roll** a custom banner. **No** sightings
of Cookiebot/CookieYes/Osano/Sourcepoint/Didomi in this set. **None** named Google Consent Mode — so
implementing v2 correctly puts Concertable *at or ahead of* the disclosed bar.

## 3. Recommendation for Concertable

**Keep the banner; make it a real, custom gate in the shared React layer — do not buy an enterprise CMP.**
Rationale:

- Concertable's **closest analogues (GigPig, GigXchange) are custom**, and GigXchange proves a small,
  modern React/Supabase app does compliant opt-in + Consent Mode v2 with a hand-built banner.
- Concertable **already controls all script loading** — Stripe and Maps are first-party code, and future
  GA4/GTM/Meta will be added by us — so the "hard part" (blocking non-essential scripts until consent)
  is straightforward here, not the maintenance sink it is on sites with sprawling third-party tags.
- OneTrust is what the **enterprise** incumbents use; it's overkill and enterprise-priced for a pre-launch
  marketplace. A lightweight CMP (Cookiebot/CookieYes/Osano) is the fallback if we'd rather rent the
  blocking + legal upkeep than own it.

**The production-ready build (Phase 3), implemented once in `app/web/shared` for all four SPAs:**

1. **Deny-by-default before hydration** — a tiny sync snippet in each `index.html` `<head>` sets Consent
   Mode v2 defaults to `denied` and reads any stored decision, *before* React mounts (no tag fires early).
2. **Consent store = single source of truth** — per-category state (`necessary` always-on / `functional` /
   `analytics` / `marketing`), persisted as a **timestamped record**. (Extends the existing `consent.ts`.)
3. **Category-keyed lazy script loader** — non-essential tech registers against a category and is injected
   only when that category flips to `granted`; nothing non-essential is a static import. This is the
   primitive that makes `hasConsent` finally *do* something.
4. **React both ways** — grant → inject + `gtag('consent','update',…'granted')`; withdraw → stop firing +
   delete dropped cookies (reload is the simplest guarantee). Persistent "Cookie settings" re-open on all
   four SPAs. Re-evaluate on SPA route changes.
5. **Reject-all at equal prominence** on the banner's first layer (the one compliance fix the current
   banner is missing), no pre-ticked toggles, consent records retained.
6. **Lazy Stripe + Maps** (separate from the banner): Stripe only at checkout (→ its cookies become
   strictly-necessary/exempt); Maps only on find/autocomplete routes (functional).
7. **Storage manifest + drift-guard test** so any new storage write must be classified to compile-green;
   the engineering inventory (Phase 4 `BROWSER_STORAGE.md`) generates from it.

Adopt Eventbrite's **"Organizer" category** idea only if B2B-context tracking later needs its own bucket.

**Open decision (yours):** custom-real-gate (recommended) vs a lightweight CMP vs enterprise OneTrust.
Everything above assumes custom; a CMP swaps steps 1–5 for vendor config but keeps 6–7.

## Sources

**Legal (primary):** [PECR reg 6](https://www.legislation.gov.uk/uksi/2003/2426/regulation/6) ·
[ICO storage & access guidance](https://ico.org.uk/for-organisations/direct-marketing-and-privacy-and-electronic-communications/guidance-on-the-use-of-storage-and-access-technologies/) ·
[ICO exceptions](https://ico.org.uk/for-organisations/direct-marketing-and-privacy-and-electronic-communications/guidance-on-the-use-of-storage-and-access-technologies/what-are-the-exceptions/) ·
[ICO manage consent](https://ico.org.uk/for-organisations/direct-marketing-and-privacy-and-electronic-communications/guidance-on-the-use-of-storage-and-access-technologies/how-do-we-manage-consent-in-practice/) ·
[Sky Betting reprimand](https://ico.org.uk/action-weve-taken/enforcement/2024/09/bonne-terre-limited/) ·
[ICO top-1,000 sweep (Dec 2025)](https://ico.org.uk/about-the-ico/media-centre/news-and-blogs/2025/12/ico-action-secures-increased-cookie-compliance/) ·
[Final guidance (Apr 2026)](https://ico.org.uk/about-the-ico/media-centre/news-and-blogs/2026/04/final-storage-and-access-technologies-guidance-published/) ·
[Google Consent Mode setup](https://developers.google.com/tag-platform/security/guides/consent) ·
[Consent Mode v2 for EEA (Ads)](https://support.google.com/google-ads/answer/13695607) ·
[Stripe cookies policy](https://stripe.com/legal/cookies-policy) ·
[DUAA 2025 cookies (Stevens & Bolton)](https://www.stevens-bolton.com/insights/102mqbh/the-data-use-and-access-act-2025-cookies-what-is-changing-and-what-you-need-t/)

**Gig marketplaces:** [GigPig](https://www.gigpig.uk/) ·
[GigPig policy](https://gigpigcdn.ams3.cdn.digitaloceanspaces.com/legal/privacy_cookie_policy.html) ·
[GigXchange](https://gigxchange.app/) · [GigXchange privacy](https://gigxchange.app/privacy) ·
[gigmit privacy](https://www.gigmit.com/en/privacy) · [Poptop privacy](https://www.poptop.uk.com/privacy-policy/) ·
[Encore privacy](https://encoremusicians.com/about/privacy) · [Alive Network cookies](https://www.alivenetwork.com/cookie-policy)

**Ticketing platforms:** [Ticketmaster cookie policy](https://privacy.ticketmaster.com/cookie-policy) ·
[Ticketmaster Business](https://business.ticketmaster.com/) · [Universe cookies](https://www.universe.com/cookies) ·
[TicketWeb cookies](https://info.ticketweb.com/cookie-policy/) · [Eventbrite cookies](https://www.eventbrite.co.uk/cookies/) ·
[Skiddle cookies](https://www.skiddle.com/terms/cookie-policy/) · [Songkick (OneTrust)](https://www.wminewmedia.com/cookies-policy/) ·
[Resident Advisor cookies](https://ra.co/cookiepolicy) · [Bandsintown cookies](https://corp.bandsintown.com/cookie-policy)

**Confidence:** live banners confirmed for GigPig, GigXchange; OneTrust confirmed for the Live Nation
estate/AXS/Songkick via `OptanonConsent`. Several ICO pages and some ticketing sites (See Tickets, AXS,
DICE) 403'd automated fetch — findings there lean on policy text + search snippets, flagged in the source
agents' notes. No CMP-vendor was branding-confirmed for GigPig (OneTrust inferred from stock copy).
