param(
    [Parameter(Position = 0)]
    [string]$cmd,
    [switch]$Headed
)

Set-Location $PSScriptRoot
[Environment]::CurrentDirectory = $PSScriptRoot

$b2bUi      = Join-Path $PSScriptRoot "api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests.Ui"
$customerUi = Join-Path $PSScriptRoot "api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests.Ui"
$baselineMd = Join-Path $PSScriptRoot "api/Tests/Concertable.E2ETests/E2E_BASELINE.md"

if (-not $Headed) { $env:HEADLESS = "true" }

function Get-BaselinePassing([string]$suite) {
    $md = Get-Content -Raw $baselineMd
    $marker = '<!-- BASELINE-DATA-START -->'
    $markerIdx = $md.IndexOf($marker)
    if ($markerIdx -lt 0) {
        throw "BASELINE FORMAT ERROR: missing '$marker' sentinel in $baselineMd. The regress parser only reads content below this marker."
    }
    $data = $md.Substring($markerIdx + $marker.Length)

    $pattern = "(?ms)### $suite passing \((\d+)\)\s*``````text\s*(.+?)\s*``````"
    $match = [regex]::Match($data, $pattern)
    if (-not $match.Success) {
        throw "BASELINE FORMAT ERROR: couldn't find '### $suite passing (N)' section followed by a ``````text block in $baselineMd. See editing rules at the top of that file."
    }

    $declared = [int]$match.Groups[1].Value
    $scenarios = $match.Groups[2].Value.Split([string[]]@("`r`n","`n"), [StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object { $_.Trim() } | Where-Object { $_ }

    if ($scenarios.Count -ne $declared) {
        throw "BASELINE FORMAT ERROR: '### $suite passing ($declared)' but the fenced block has $($scenarios.Count) scenarios. Update either the (N) in the heading or the line count."
    }

    foreach ($s in $scenarios) {
        if ($s -match '^[-*•]\s' -or $s -match '^["''`]') {
            throw "BASELINE FORMAT ERROR: '$suite passing' contains a bulleted/quoted scenario: '$s'. Plain text only -- see editing rules."
        }
    }
    return $scenarios
}

function Invoke-Regress([string]$suite, [string]$csproj) {
    Write-Host ""
    Write-Host "=== Regress: $suite ===" -ForegroundColor Cyan

    $expected = Get-BaselinePassing $suite
    Write-Host "Baseline says $($expected.Count) $suite scenarios must pass." -ForegroundColor Gray
    $filter = ($expected | ForEach-Object { "DisplayName=$_" }) -join '|'

    # Preflight: confirm each baseline scenario resolves to a real test
    $list = dotnet test $csproj --list-tests --filter $filter 2>&1
    $discovered = $list | Where-Object { $_ -match '^\s{4}\S' } | ForEach-Object { $_.Trim() }
    if ($discovered.Count -ne $expected.Count) {
        $missing = $expected | Where-Object { $discovered -notcontains $_ }
        Write-Host "BASELINE DRIFT: dotnet test --list-tests discovered $($discovered.Count) of $($expected.Count) expected $suite scenarios." -ForegroundColor Red
        Write-Host "Missing (renamed in .feature, or typo'd in baseline?):" -ForegroundColor Red
        $missing | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
        throw "Baseline drift -- fix $baselineMd or the .feature file."
    }
    Write-Host "Preflight OK: all $($expected.Count) baseline scenarios resolve to real tests." -ForegroundColor Gray

    # Run them
    $logPath = (Join-Path (Split-Path $csproj -Parent) 'regress.last.log')
    dotnet test $csproj --filter $filter --logger "console;verbosity=normal" 2>&1 | Tee-Object -FilePath $logPath

    # Parse pass/fail totals (Tee writes UTF-16 on Windows)
    $logBytes = [System.IO.File]::ReadAllBytes($logPath)
    $logText = if ($logBytes.Length -ge 2 -and $logBytes[0] -eq 0xFF -and $logBytes[1] -eq 0xFE) {
        [System.Text.Encoding]::Unicode.GetString($logBytes)
    } else {
        [System.Text.Encoding]::UTF8.GetString($logBytes)
    }
    # dotnet test prints "Failed: N" only when N > 0
    $passedMatch = [regex]::Match($logText, '(?m)^\s*Passed:\s*(\d+)')
    $failedMatch = [regex]::Match($logText, '(?m)^\s*Failed:\s*(\d+)')
    if (-not $passedMatch.Success) {
        throw "Couldn't parse 'Passed: N' from $logPath -- did the run complete?"
    }
    $passed = [int]$passedMatch.Groups[1].Value
    $failed = if ($failedMatch.Success) { [int]$failedMatch.Groups[1].Value } else { 0 }

    if ($passed -eq $expected.Count -and $failed -eq 0) {
        Write-Host "  $suite OK: $passed/$($expected.Count) passed." -ForegroundColor Green
        return $true
    }

    $failedNames = [regex]::Matches($logText, '(?m)^\s+Failed\s+(.+?)\s+\[') | ForEach-Object { $_.Groups[1].Value }
    Write-Host "  $suite REGRESSED: passed $passed of $($expected.Count), failed $failed." -ForegroundColor Red
    Write-Host "  Failing scenarios:" -ForegroundColor Red
    $failedNames | ForEach-Object { Write-Host "    - $_" -ForegroundColor Red }
    return $false
}

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
    "regress" {
        $b2bOk  = Invoke-Regress 'B2B'      "$b2bUi/Concertable.B2B.E2ETests.Ui.csproj"
        $custOk = Invoke-Regress 'Customer' "$customerUi/Concertable.Customer.E2ETests.Ui.csproj"
        Write-Host ""
        if ($b2bOk -and $custOk) {
            Write-Host "REGRESS PASSED -- every baseline-passing scenario still passes." -ForegroundColor Green
            exit 0
        } else {
            Write-Host "REGRESS FAILED -- at least one baseline-passing scenario regressed." -ForegroundColor Red
            exit 1
        }
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
        Write-Host "    regress   Run only baseline-passing scenarios; fail on any regression (~3 min)"
        Write-Host "    b2b       Run B2B UI E2E tests only"
        Write-Host "    customer  Run Customer UI E2E tests only"
        Write-Host "    3ds       Run 3DS scenarios (B2B only)"
        Write-Host "    trace     Open latest Playwright trace"
        Write-Host ""
    }
}

Remove-Item Env:\HEADLESS -ErrorAction SilentlyContinue
