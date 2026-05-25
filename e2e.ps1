param(
    [Parameter(Position = 0)]
    [string]$cmd,
    [Parameter(Position = 1, ValueFromRemainingArguments)]
    [string[]]$args
)

$e2eUi = "api/Tests/Concertable.E2ETests/Concertable.E2ETests.Ui"

switch ($cmd) {
    "run"   { & "$e2eUi/run-ui-tests.ps1" @args }
    "3ds"   { & "$e2eUi/Run-3dsTests.ps1" @args }
    "trace" { & "api/Tests/Concertable.E2ETests/ui-trace.ps1" }
    default {
        Write-Host ""
        Write-Host "  Usage: ./e2e.ps1 <command> [options]" -ForegroundColor White
        Write-Host ""
        Write-Host "  Commands:" -ForegroundColor DarkGray
        Write-Host "    run    [-Headed]                  Run UI E2E tests"
        Write-Host "    3ds    [-SuccessOnly] [-Headless]  Run 3DS E2E tests"
        Write-Host "    trace                             Open latest Playwright trace"
        Write-Host ""
    }
}
