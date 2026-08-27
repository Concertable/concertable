# Running the app locally

CI and the E2E suites set their own environment and secrets, so they never hit this. A **fresh checkout
or a fresh worktree cannot run any AppHost interactively** until you do a one-time setup, because two
pieces of config live outside git by design:

1. **SPA CORS / OIDC-redirect config** — `appsettings.Development.json` next to each service's
   `appsettings.json`. Gitignored (`**/appsettings.Development.json`). Without it every SPA login
   CORS-fails at `/.well-known/openid-configuration` (Auth) or `/api/auth/me` (the data service) — looks
   like a broken feature, is a missing file.
2. **ServiceAuth client secrets** — `dotnet user-secrets` on the AppHosts. Without them Auth crashes at
   startup: `InvalidOperationException: Configuration 'ServiceAuth:B2BClientSecret' is required.`

## One-time setup

```powershell
./scripts/setup-local-dev.ps1
```

Idempotent — creates only what's missing, never overwrites. It:

- copies `appsettings.Development.json.example` → `appsettings.Development.json` for `Concertable.Auth`,
  `Concertable.B2B.Web`, `Concertable.Customer.Web` (checked-in templates, no secrets — just
  `https://localhost:517x` origins);
- sets `ServiceAuth:{B2B,Customer,Auth}ClientSecret` user-secrets on `Concertable.AppHost`,
  `Concertable.B2B.AppHost`, `Concertable.Customer.AppHost` to a shared dev value (not a secret — Auth
  and every service read the same value from the same AppHost config, and it never leaves localhost).

`user-secrets` storage is machine-wide, so step 2 is genuinely once-per-machine; the
`appsettings.Development.json` files are per-worktree (gitignored), so re-run the script (or copy the
files) in each new worktree.

### Stripe (optional)

Only needed for payment / settlement / webhook flows. `setup-local-dev.ps1` does **not** set it. If you
need it, use your own Stripe **test** key (same account as `pk_test_...` in `app/web/.env.development`):

```powershell
dotnet user-secrets set Stripe:SecretKey sk_test_xxx --project api/Concertable.B2B/src/Concertable.B2B.AppHost
```

Without it the `stripe-cli` resource is skipped and everything else runs.

## Running

```powershell
dotnet run --project api/Concertable.AppHost                       # the whole platform
dotnet run --project api/Concertable.B2B/src/Concertable.B2B.AppHost   # just the B2B slice (+ Auth, Payment)
```

The Aspire dashboard opens with links to every service and SPA. The SPAs are started by the AppHost.

### Port map

| Service | URL | SPA | URL |
|---|---|---|---|
| B2B API | `https://localhost:7086` | customer | `https://localhost:5174` |
| Search API | `https://localhost:7087` | venue | `https://localhost:5175` |
| Payment API | `https://localhost:7088` | artist | `https://localhost:5176` |
| Customer API | `https://localhost:7090` | business | `https://localhost:5177` |
| Auth | `https://localhost:7083` | admin | `https://localhost:5178` |

## Gotchas

- **Deeply-nested worktree + `LongPathsEnabled=0`** — a build can fail with
  `MSB3030: Could not copy the file "obj\Debug\net10.0\X.dll" because it was not found` on the
  longest-named projects (`Concertable.Shared.Notification.Infrastructure`,
  `Concertable.Customer.DataAccess.Infrastructure`) when the `obj` DLL path exceeds 260 chars. Enable long
  paths (`reg add HKLM\SYSTEM\CurrentControlSet\Control\FileSystem /v LongPathsEnabled /t REG_DWORD /d 1`,
  admin, then reboot), or run from a shallower path (the main checkout, not a
  `.worktrees/Long-Branch-Name/` one).
- **SQL data volume** is isolated per worktree automatically (hashed on the AppHost working directory) —
  no manual naming needed since 2026-08-22.
- `appsettings.E2E.json` **is** committed (it holds only fixed-localhost E2E config, no secrets) — don't
  confuse it with `appsettings.Development.json`.

## Follow-up

`setup-local-dev.ps1` doesn't yet verify the OIDC dev-cert trust (`dotnet dev-certs https --trust`) or
check for a running Docker engine before an AppHost start. Add those if this keeps biting.
