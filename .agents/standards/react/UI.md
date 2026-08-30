# UI and styling

Tailwind for appearance, copy-in primitives for behaviour, one library per cross-cutting UI job. The rule
this file exists to hold: **there is exactly one way to express a variant, one place classes merge, and one
owner for each primitive.**

## Classes merge in `cn()`, nowhere else

`cn()` is clsx plus tailwind-merge, so a later utility actually beats an earlier one instead of both
landing in the class string and the loser winning by source order.

```ts
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}
```

Every component that accepts a `className` passes it through `cn()` last, so a caller can always override.
A component that concatenates class strings by hand, or that drops the caller's `className`, is a bug.

## Variants are `cva`, not stacked ternaries

A component with more than one visual axis (variant, size, tone) declares them once with **cva** and takes
the generated props. Ternaries in the class string are how two axes become an unreadable product of four.

```ts
const badgeVariants = cva("inline-flex items-center rounded-md border px-2 py-0.5", {
  variants: { variant: { default: "…", destructive: "…" } },
  defaultVariants: { variant: "default" },
});
```

## Copy-in primitives are owned code

Primitives generated into `components/ui/` (the shadcn model over Radix) are **your files**. Edit them in
place when the design needs it; they are not vendored dependencies to leave untouched and wrap.

Two rules keep that from turning into drift:

- **A primitive is generic.** No feature vocabulary, no domain type, no data fetching. The moment a
  primitive imports from a feature, it stopped being a primitive.
- **A feature never re-implements one.** If the button needs a new tone, the button gains a variant.

Do not add a second component library alongside the primitives already in the tree, and do not add a
CSS-in-JS runtime — the point of utilities plus owned primitives is that there is one place to look.

## One icon set, one toast library

Pick a single icon package and import from it directly; mixing sets is visible on screen. Toasts come from
one library, mounted once at the root.

**Fire the toast from the hook that owns the operation, not from the component that rendered the button.**
The mutation knows whether it succeeded; the component should not have to. That also keeps the message from
appearing twice when two components trigger the same mutation.

## Animation is opt-in and local

An animation library earns its place at genuine entrance/exit and layout transitions. Reach for a CSS
transition first — most hover, focus and open/close states need nothing more.

Never animate on every render, and never make an animation load-bearing for whether content is reachable:
if the library fails to load, the page must still show its content.

## Dark mode and tokens

Colours come from CSS variables the theme defines, not from literal palette utilities scattered through
components. A component that hard-codes a specific shade renders wrong in the other theme, and the failure
shows up only in whichever mode the author was not using.
