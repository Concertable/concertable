# Dates and times

dayjs, and **every format string lives in one formatting module**. The failure this prevents is mundane and
constant: the same timestamp rendering three ways on one screen because three components each chose their
own format.

## One module owns formatting

Export named functions, not format strings. A caller asks for the *meaning* it wants rendered; it does not
know the pattern.

```ts
export const formatEventDate = (value: string) => dayjs(value).format("ddd D MMM YYYY");
export const formatEventTime = (value: string) => dayjs(value).format("HH:mm");
```

**`dayjs(x).format("…")` inline in a component is the anti-pattern**, even once — it is how the second
variant of a format gets written without anyone seeing the first. Changing how dates look should be one
diff in one file.

The same module owns relative time ("3 days ago") and range rendering ("12–14 May"), for the same reason.

## The wire is ISO, the screen is formatted

A date crossing the network is an ISO 8601 string, and it stays a string until something renders it.
Parse at the point of display, not at the point of receipt — a `Date` stored in state or in a store
serializes inconsistently, compares by reference, and drifts across a reload.

Where the backend sends UTC, convert to local exactly once, in the formatting module. Never assume the
device's zone matches the event's; if a time belongs to a place, the place's zone travels with it.

## Arithmetic goes through the library

`add`, `subtract`, `diff`, `startOf`, `isBefore` — all dayjs. Hand-rolled millisecond arithmetic on
`Date` is where daylight-saving bugs live, and they surface twice a year in whichever timezone the author
did not have.

Comparisons use dayjs too: `a.isBefore(b)`, not `new Date(a) < new Date(b)`.

## Plugins are registered once

dayjs ships a minimal core; `relativeTime`, `utc`, `timezone`, `customParseFormat` are plugins. Register
them in the formatting module beside the functions that need them, never scattered across features — a
plugin registered in a component that happens to render first is a load-order dependency waiting to break.

## Deliberately not used

moment (unmaintained and large), a second date library alongside dayjs, and ad-hoc
`toLocaleDateString`/`toLocaleTimeString` calls in components — the last one looks free and is exactly the
scattering this file exists to stop. `Intl` behind the formatting module is fine; `Intl` in a component is
not.
