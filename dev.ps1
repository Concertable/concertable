param(
    [Parameter(Position = 0)]
    [string]$cmd,
    [Parameter(Position = 1, ValueFromRemainingArguments)]
    [string[]]$args
)

function Show-Usage {
    Write-Host ""
    Write-Host "  Usage: ./dev.ps1 <command> [options]" -ForegroundColor White
    Write-Host ""
    Write-Host "  Commands:" -ForegroundColor DarkGray
    Write-Host "    migrations    Nuke and re-scaffold all migrations"
    Write-Host "    list          List all available commands"
    Write-Host ""
    Write-Host "  For E2E test commands use ./e2e.ps1" -ForegroundColor DarkGray
    Write-Host ""
}

switch ($cmd) {
    "migrations" { & "api/initial-migrations.ps1" }
    { $_ -in "list","help" } { Show-Usage }
    default { Show-Usage }
}
