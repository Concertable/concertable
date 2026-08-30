# The write boundary

Every user-editable form validates its controlled-input buffer against a **zod** schema at submit and maps
the **parsed** result — never the raw buffer — to the `XRequest`. A form with free-typed fields and no
schema is the violation.

One schema does two jobs a hand-rolled `if` cannot do together:

- **It narrows the type at the boundary.** `parsed.data` is proven present and correctly typed, so mapping
  to the request needs no `!` bang and no `?? fallback`. **The non-null assertion *is* the missing
  validation** — a schema removes it honestly instead of asserting past it.
- **It feeds inline field errors.** `safeParse` yields per-field messages the component renders next to each
  input, plus a derived `isValid` that gates the submit button, so React state reflects *actual* validity —
  the UX a server 400 can only deliver after a round trip.

```ts
const parsed = updateOrderRequestSchema.safeParse(buffer);
if (!parsed.success) return parsed;        // the component renders parsed.error.issues inline
updateOrder(parsed.data);                  // parsed.data IS UpdateOrderRequest — no bang
```

The schema lives in `features/<feature>/schemas/`. Keep it aligned to the request with
`type XRequest = z.infer<typeof xRequestSchema>`, which makes drift a compile error, while the naming and
camelCase wire rules still hold.

## Reshape on the way *into* the parse, never on the way out

The example above passes `buffer` straight to `safeParse` because its shape already matches the request.
Usually it does not: inputs are flat because that is how a form renders, and the request is nested because
that is how the API models it.

**Build the request shape as the argument to `safeParse`.** The schema then validates the thing actually
being sent, `z.infer` still equals `XRequest`, and `parsed.data` goes to the mutation untouched.

```ts
const parsed = updateOrderRequestSchema.safeParse({
  reference: buffer.reference,
  delivery: {
    line1: buffer.line1,
    line2: buffer.line2 || undefined,      // an empty input is absent, not ""
    postcode: buffer.postcode,
  },
  giftNote: buffer.isGift ? buffer.giftNote : undefined,
});
if (!parsed.success) return parsed;
updateOrder(parsed.data);
```

**Mapping after the parse is the mistake**, and it is a quiet one: it puts a second shape between the
validated data and the wire, so the thing you proved correct is not the thing you send. Every conditional
drop (`isGift ? … : undefined`) and every empty-string-to-`undefined` normalization belongs in this
argument too — they change what is valid, so they must happen before validation, not after it.

The reshape lives in the feature's facade hook beside the `safeParse` call, not in the component. A
component that assembles a nested request object is holding contract knowledge it should not have.

**Client validation is a UX affordance, not a trust boundary.** The server re-validates every field
regardless. Never drop a server check because the client has one.

**The anti-patterns:**

- **Raw buffer to request with a `!` bang or a `?? fallback`.** The bang is the missing parse.
- **A form with free-typed fields and no schema** — no `schemas/` folder for a feature with editable inputs
  is the tell.
- **Client validation reported by a toast** instead of inline from the parse result.

**Litmus:** *a field the user can type into, with nothing parsing it before the mutation call? → add the
schema; the parsed output, not the buffer, becomes the request.*
