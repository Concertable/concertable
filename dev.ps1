param(
    [Parameter(Position = 0)]
    [string]$cmd,
    [Parameter(Position = 1, ValueFromRemainingArguments)]
    [string[]]$args
)

$e2eUi = "api/Tests/Concertable.E2ETests/Concertable.E2ETests.Ui"

switch ($cmd) {
    "ui-tests"    { & "$e2eUi/run-ui-tests.ps1" @args }
    "3ds-tests"   { & "$e2eUi/Run-3dsTests.ps1" @args }
    "ui-trace"    { & "api/Tests/Concertable.E2ETests/ui-trace.ps1" }
    "migrations"  { & "api/initial-migrations.ps1" }
    default {
        Write-Host ""
        Write-Host "  Usage: ./dev.ps1 <command> [options]" -ForegroundColor White
        Write-Host ""
        Write-Host "  Commands:" -ForegroundColor DarkGray
        Write-Host "    ui-tests   [-Headed]              Run UI E2E tests"
        Write-Host "    3ds-tests  [-SuccessOnly] [-Headless]  Run 3DS E2E tests"
        Write-Host "    ui-trace                          Open latest Playwright trace"
        Write-Host "    migrations                        Nuke and re-scaffold all migrations"
        Write-Host ""
    }
}
