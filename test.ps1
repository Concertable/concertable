param(
    [Parameter(Position = 0)]
    [string]$cmd,
    [Parameter(Position = 1, ValueFromRemainingArguments)]
    [string[]]$rest
)

Set-Location $PSScriptRoot
[Environment]::CurrentDirectory = $PSScriptRoot

$psExe = (Get-Process -Id $PID).Path
if (-not $psExe) { $psExe = if ($PSVersionTable.PSVersion.Major -ge 6) { 'pwsh' } else { 'powershell' } }

function Invoke-Suite([string]$label, [string]$script, [string[]]$scriptArgs) {
    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Magenta
    Write-Host "  $label" -ForegroundColor Magenta
    Write-Host "============================================================" -ForegroundColor Magenta
    & $psExe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot $script) @scriptArgs | Out-Host
    return $LASTEXITCODE
}

function Forward([string]$label, [string]$script, [string[]]$scriptArgs) {
    if (-not $scriptArgs -or $scriptArgs.Count -eq 0) { $scriptArgs = @('run') }
    $code = Invoke-Suite $label $script $scriptArgs
    exit $code
}

function Show-Summary([object[]]$summaries) {
    Write-Host ""
    Write-Host "  Test summary" -ForegroundColor Cyan
    Write-Host ("  {0,-14}{1,10}" -f 'Suite', 'Result') -ForegroundColor Gray
    Write-Host ("  {0,-14}{1,10}" -f '------', '------') -ForegroundColor Gray
    foreach ($s in $summaries) {
        $ok = $s.ExitCode -eq 0
        $color = if ($ok) { 'Green' } else { 'Red' }
        $text = if ($ok) { 'PASS' } else { 'FAIL' }
        Write-Host ("  {0,-14}{1,10}" -f $s.Suite, $text) -ForegroundColor $color
    }
    Write-Host ""
}

function Show-Usage {
    Write-Host ""
    Write-Host "  Usage: ./test.ps1 <command> [-- <args forwarded to the suite script>]" -ForegroundColor White
    Write-Host ""
    Write-Host "  Commands:" -ForegroundColor DarkGray
    Write-Host "    all           Run unit + integration + e2e, then a combined PASS/FAIL banner"
    Write-Host "    unit          Run all unit tests        (forwards to ./unit.ps1)"
    Write-Host "    integration   Run all integration tests (forwards to ./integration.ps1)"
    Write-Host "    e2e           Run all UI E2E tests       (forwards to ./e2e.ps1)"
    Write-Host "    list          Show this help"
    Write-Host ""
    Write-Host "  Notes:" -ForegroundColor DarkGray
    Write-Host "    'all' runs e2e via 'regress' (the baseline-passing set) so it can fold a"
    Write-Host "    real pass/fail into the banner. For the full discovery run use ./test.ps1 e2e run."
    Write-Host "    Any extra args are forwarded, e.g. ./test.ps1 unit b2b  or  ./test.ps1 e2e run -Headed"
    Write-Host ""
}

switch ($cmd) {
    "all" {
        $results = @()
        $results += [pscustomobject]@{ Suite = 'unit';        ExitCode = (Invoke-Suite 'Unit tests'        'unit.ps1'        @('run')) }
        $results += [pscustomobject]@{ Suite = 'integration'; ExitCode = (Invoke-Suite 'Integration tests' 'integration.ps1' @('run')) }
        $results += [pscustomobject]@{ Suite = 'e2e';         ExitCode = (Invoke-Suite 'E2E tests (regress)' 'e2e.ps1'        @('regress')) }
        Show-Summary $results
        if ($results | Where-Object { $_.ExitCode -ne 0 }) {
            Write-Host "  TESTS FAILED -- at least one suite did not pass." -ForegroundColor Red
            exit 1
        }
        Write-Host "  ALL TESTS PASSED." -ForegroundColor Green
        exit 0
    }
    "unit"        { Forward 'Unit tests'        'unit.ps1'        $rest }
    "integration" { Forward 'Integration tests' 'integration.ps1' $rest }
    "e2e"         { Forward 'E2E tests'          'e2e.ps1'         $rest }
    { $_ -in "list","help" } { Show-Usage }
    default       { Show-Usage }
}
