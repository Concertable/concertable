param(
    [Parameter(Position = 0)]
    [string]$cmd,
    [Parameter(Position = 1, ValueFromRemainingArguments)]
    [string[]]$args
)

switch ($cmd) {
    "migrations" { & "api/initial-migrations.ps1" }
    default {
        Write-Host ""
        Write-Host "  Usage: ./dev.ps1 <command> [options]" -ForegroundColor White
        Write-Host ""
        Write-Host "  Commands:" -ForegroundColor DarkGray
        Write-Host "    migrations    Nuke and re-scaffold all migrations"
        Write-Host ""
        Write-Host "  For E2E test commands use ./e2e.ps1" -ForegroundColor DarkGray
        Write-Host ""
    }
}
