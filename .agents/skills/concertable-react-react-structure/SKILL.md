---
name: concertable-react-react-structure
description: Concertable routes mount-only Effects through the shared `useMountEffect` hook, for the narrow case of syncing with something outside React — never as a way to fetch, since server data is a query. Use when reaching for a mount-only `useEffect` here, or reviewing one that fetches.
---

# Structure — Concertable's mount-only Effect helper

The generic standard is the `react-structure` skill: the feature slice, hooks orchestrate and components
render, the raw-hook versus facade-hook split, and what an Effect is actually for.

Mount-only Effects go through **`useMountEffect`** (`app/shared/src/hooks/useMountEffect.ts`) — for the narrow
case the skill sanctions, syncing with something outside React.

It is not a way to fetch. Server data is a query; that is the `server-state` skill.
