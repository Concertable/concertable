param(
    [Parameter(Position = 0)]
    [string]$cmd,
    [switch]$Headed
)

$b2bUi      = "api/Tests/Concertable.B2B.E2ETests.Ui"
$customerUi = "api/Tests/Concertable.Customer.E2ETests.Ui"

if (-not $Headed) { $env:HEADLESS = "true" }

switch ($cmd) {
    "run" {
        dotnet test "$b2bUi/Concertable.B2B.E2ETests.Ui.csproj" --logger "console;verbosity=normal" | Tee-Object -FilePath "$b2bUi/ui-tests.last.log"
        dotnet test "$customerUi/Concertable.Customer.E2ETests.Ui.csproj" --logger "console;verbosity=normal" | Tee-Object -FilePath "$customerUi/ui-tests.last.log"
    }
    "b2b" {
        dotnet test "$b2bUi/Concertable.B2B.E2ETests.Ui.csproj" --logger "console;verbosity=normal" | Tee-Object -FilePath "$b2bUi/ui-tests.last.log"
    }
    "customer" {
        dotnet test "$customerUi/Concertable.Customer.E2ETests.Ui.csproj" --logger "console;verbosity=normal" | Tee-Object -FilePath "$customerUi/ui-tests.last.log"
    }
    "3ds" {
        dotnet test "$b2bUi/Concertable.B2B.E2ETests.Ui.csproj" --filter "DisplayName~3DS" --logger "console;verbosity=normal" | Tee-Object -FilePath "$b2bUi/ui-tests.last.log"
    }
    "trace" { & "api/Tests/Concertable.E2ETests/ui-trace.ps1" }
    default {
        Write-Host ""
        Write-Host "  Usage: ./e2e.ps1 <command> [-Headed]" -ForegroundColor White
        Write-Host ""
        Write-Host "  Commands:" -ForegroundColor DarkGray
        Write-Host "    run       Run all UI E2E tests (B2B + Customer)"
        Write-Host "    b2b       Run B2B UI E2E tests only"
        Write-Host "    customer  Run Customer UI E2E tests only"
        Write-Host "    3ds       Run 3DS scenarios (B2B only)"
        Write-Host "    trace     Open latest Playwright trace"
        Write-Host ""
    }
}

Remove-Item Env:\HEADLESS -ErrorAction SilentlyContinue
