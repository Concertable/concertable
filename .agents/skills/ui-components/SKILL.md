---
name: ui-components
description: Tailwind styling and the component primitives around it — class strings merged only through a cn() helper that lets a caller's className win, visual variants expressed with cva rather than stacked ternaries, copy-in Radix/shadcn primitives treated as owned code that stays generic while features never re-implement one, a single icon set, toasts fired from the hook that owns the mutation rather than the component that rendered the button, animation kept opt-in and never load-bearing for reachability, and theme colours taken from CSS variables so the other mode is not broken silently. Use when building or restyling a component, adding a variant, editing a generated primitive, adding an icon or toast or animation library, or reviewing a class string that concatenates by hand.
---

# ui-components

The standard is `../../standards/react/UI.md`, shipped in this plugin. Read it and follow it; this skill only routes to it.
