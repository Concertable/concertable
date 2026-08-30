---
name: routing
description: File-based routing with TanStack Router — a route file wires a route and delegates the screen to its feature slice, access control is a beforeLoad guard written as an exported requireX function that throws redirect rather than a wrapper component that mounts first, the guard warms the query cache with ensureQueryData instead of a loader that duplicates what the component re-reads, search params are parsed with validateSearch and read through the typed useSearch because a URL is untrusted input, navigation is typed links and navigate rather than concatenated strings, and a pathless layout route owns the chrome and the guard that covers a whole section. Use when adding or moving a route, protecting a page, deciding between a loader and a query, reading or writing anything in the URL, or reviewing a route file that has grown a feature inside it.
---

# routing

The standard is `../../standards/react/ROUTING.md`, shipped in this plugin. Read it and follow it; this skill only routes to it.
