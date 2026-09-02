<#
setup-local-dev.ps1 — one-time (per machine + per fresh worktree) local-run bootstrap.

WHY THIS EXISTS
A fresh checkout cannot run any AppHost interactively: the SPA CORS / OIDC-redirect
config and the ServiceAuth client secrets live outside git (gitignored
appsettings.Development.json + `dotnet user-secrets`), so `dotnet run` on an AppHost
either has Auth crash at startup ("Configuration 'ServiceAuth:B2BClientSecret' is
required.") or every SPA login CORS-fails. CI/E2E don't hit this — they set their own
environment and secrets. See docs/LOCAL_DEV.md.

This script is idempotent: it only creates what's missing and never overwrites an
existing file or secret.

  ./scripts/setup-local-dev.ps1            # do it
  ./scripts/setup-local-dev.ps1 -WhatIf    # show what it would do
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent

function Copy-DevSettings([string]$dir) {
    $target = Join-Path $repoRoot "$dir/appsettings.Development.json"
    $example = "$target.example"
    if (-not (Test-Path $example)) { Write-Warning "no template at $example — skipping"; return }
    if (Test-Path $target) {
        Write-Host "  exists, left as-is : $dir/appsettings.Development.json" -ForegroundColor DarkGray
        return
    }
    if ($PSCmdlet.ShouldProcess("$dir/appsettings.Development.json", "create from .example")) {
        Copy-Item $example $target
        Write-Host "  created            : $dir/appsettings.Development.json" -ForegroundColor Green
    }
}

# Any consistent non-empty value works: Auth and every data service read the SAME
# value from the SAME AppHost config, so they always agree. This is NOT a secret —
# it never leaves localhost and grants nothing off-box.
$devServiceSecret = 'local-dev-shared-service-secret'
$serviceAuthKeys = @(
    'ServiceAuth:B2BClientSecret'
    'ServiceAuth:CustomerClientSecret'
    'ServiceAuth:AuthClientSecret'
)
# UserSecretsId storage is machine-wide (outside any worktree) — set once, every
# checkout picks it up. Each AppHost that boots Auth needs all three.
$appHosts = @(
    'api/Concertable.AppHost'
    'api/Concertable.B2B/src/Concertable.B2B.AppHost'
    'api/Concertable.Customer/src/Concertable.Customer.AppHost'
)

function Set-ServiceAuthSecrets([string]$appHostDir) {
    $proj = Join-Path $repoRoot $appHostDir
    $existing = ''
    try { $existing = (& dotnet user-secrets list --project $proj) -join "`n" } catch { }
    foreach ($key in $serviceAuthKeys) {
        if ($existing -match [regex]::Escape("$key = ")) {
            Write-Host "  set, left as-is    : $appHostDir  $key" -ForegroundColor DarkGray
            continue
        }
        if ($PSCmdlet.ShouldProcess("$appHostDir  $key", "dotnet user-secrets set")) {
            & dotnet user-secrets set $key $devServiceSecret --project $proj | Out-Null
            Write-Host "  set                : $appHostDir  $key" -ForegroundColor Green
        }
    }
}

Write-Host ""
Write-Host "1. SPA CORS / OIDC-redirect config (gitignored appsettings.Development.json)" -ForegroundColor White
Copy-DevSettings 'api/Concertable.Auth/src/Concertable.Auth'
Copy-DevSettings 'api/Concertable.B2B/src/Concertable.B2B.Web'
Copy-DevSettings 'api/Concertable.Customer/src/Concertable.Customer.Web'

Write-Host ""
Write-Host "2. ServiceAuth client secrets (dotnet user-secrets, machine-wide)" -ForegroundColor White
foreach ($h in $appHosts) { Set-ServiceAuthSecrets $h }

Write-Host ""
Write-Host "3. Stripe (OPTIONAL — only for payment / settlement / webhook flows)" -ForegroundColor White
Write-Host "   Not set by this script. If you need it, use YOUR own Stripe test key" -ForegroundColor DarkGray
Write-Host "   (same account as pk_test_... in app/web/.env.development):" -ForegroundColor DarkGray
Write-Host "     dotnet user-secrets set Stripe:SecretKey sk_test_xxx --project api/Concertable.B2B/src/Concertable.B2B.AppHost" -ForegroundColor DarkGray
Write-Host "   Without it the stripe-cli resource is skipped and the rest of the app runs fine." -ForegroundColor DarkGray

Write-Host ""
Write-Host "Done. Run an AppHost with:  dotnet run --project api/Concertable.AppHost" -ForegroundColor Cyan
Write-Host "(or api/Concertable.B2B/src/Concertable.B2B.AppHost for just the B2B slice)" -ForegroundColor DarkGray
Write-Host ""
