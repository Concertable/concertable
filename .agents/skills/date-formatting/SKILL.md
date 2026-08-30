---
name: date-formatting
description: Dates and times with dayjs behind one formatting module — named functions rather than format strings, with an inline dayjs().format() in a component treated as the anti-pattern that produces three renderings of one timestamp on a screen, ISO strings on the wire that stay strings until display rather than Date objects held in state or a store, a single conversion out of UTC inside that module, all arithmetic and comparison through the library instead of hand-rolled millisecond maths that breaks twice a year, plugins registered once beside the functions that need them, and moment plus scattered toLocaleDateString calls deliberately not used. Use when rendering a date or time, adding a format, doing date arithmetic or comparison, storing a timestamp in state, or registering a dayjs plugin.
---

# date-formatting

The standard is `../../standards/react/DATES.md`, shipped in this plugin. Read it and follow it; this skill only routes to it.
