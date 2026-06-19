# Concertable — cross-cutting technical debt

Debt spanning multiple services or living in shared code (`Shared/`, `Concertable.Messaging`, host `Program.cs` files). Service-specific debt belongs in that service's own `TECH_DEBT.md`. When an item is fixed, update both this file and [`ARCHITECTURE.md`](./ARCHITECTURE.md).

---

## MED

### Kernel `ClaimsPrincipal.GetId()` fails open with `string.Empty`

`Shared/Concertable.Kernel/Identity/ClaimsPrincipalExtensions.cs` returns `user?.FindFirst("sub")?.Value ?? string.Empty` — a principal with no `sub` claim becomes an empty-string user id instead of a failure. Its sibling `CurrentUserExtensions.GetId(ICurrentUser)` gets this right (throws `UnauthorizedAccessException`). The only consumer, `NotificationHub`, assigns the result to `string?` and null-checks it — a check that can never fire because the method never returns null, so an unauthenticated principal sails through as `""`.

**Resolves when:** the extension fails closed (returns `string?` with no empty-string coercion, or throws like its `ICurrentUser` sibling), and `NotificationHub`'s guard actually rejects principals without a `sub` claim.

---

### Required config bound with `?? ""` — services boot misconfigured and fail later

Eight hosts coalesce required auth/bus settings to empty string at bind time: `Auth:Authority` / `ServiceAuth:*` `ClientId`+`ClientSecret` in `Concertable.Auth/Program.cs`, `Concertable.B2B.Web/Program.cs`, `Concertable.B2B.Workers/ServiceCollectionExtensions.cs`, `Concertable.Customer.Web/Program.cs`; the ASB `ConnectionString` additionally in `Concertable.Payment.Web`, `Concertable.Payment.Workers`, `Concertable.Search.Workers`, and `Concertable.B2B.Seed.Simulator` `Program.cs`. A missing setting silently becomes `""`, the host starts cleanly, and the failure surfaces later as a confusing auth/bus error instead of at startup. `Concertable.Messaging.AzureServiceBus/Options/AzureServiceBusOptions.cs` compounds it with `= ""` property defaults where the convention (`docs/CODE_CONVENTIONS.md`) requires `null!` for binder-populated values. All of these also use the banned `""` literal.

**Resolves when:** required settings fail fast at startup — options validation with `ValidateOnStart` (or an explicit throw on missing key) replaces every `?? ""`, and `AzureServiceBusOptions` defaults become `null!`. Genuine optional-with-empty-default settings, if any, keep an explicit `string.Empty`.
