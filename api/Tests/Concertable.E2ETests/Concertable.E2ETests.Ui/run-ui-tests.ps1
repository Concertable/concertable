param(
    [switch]$Headed
)

$env:HEADLESS = if ($Headed) { "false" } else { "true" }

$logFile  = Join-Path $PSScriptRoot "ui-tests.last.log"
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..\..\..")).Path
$lines    = [System.Collections.Generic.List[string]]::new()

Write-Host ""
Write-Host "  Building..." -ForegroundColor DarkGray

try {
    dotnet test "$PSScriptRoot/Concertable.E2ETests.Ui.csproj" --logger "console;verbosity=normal" | ForEach-Object {
        $lines.Add($_)
        if ($_ -match '^\s+Passed ') {
            Write-Host $_ -ForegroundColor Green
        } elseif ($_ -match '^\s+Failed ') {
            Write-Host $_ -ForegroundColor Red
        } elseif ($_ -match ':\s*error\s') {
            Write-Host $_ -ForegroundColor Red
        } elseif ($_ -match 'Build succeeded') {
            Write-Host ""
            Write-Host "  Running tests..." -ForegroundColor DarkGray
            Write-Host ""
        }
    }
    $exitCode = $LASTEXITCODE

    Set-Content -Path $logFile -Value $lines -Encoding utf8

    # --- Parse failures: error message + own-code stack frames only ---
    $failures = @()
    $current  = $null
    $state    = 'none'

    foreach ($line in $lines) {
        if ($line -match '^\s+Failed\s+(.+?)\s+\[[^\]]*\]\s*$') {
            if ($current) { $failures += $current }
            $current = [pscustomobject]@{ Test = $Matches[1].Trim(); Message = @(); Stack = @() }
            $state = 'afterFailed'
            continue
        }
        if ($line -match '^\s+Passed\s') {
            if ($current) { $failures += $current; $current = $null }
            $state = 'none'
            continue
        }
        if ($null -eq $current) { continue }
        if ($line -match '^\s*Error Message:\s*$')            { $state = 'msg';   continue }
        if ($line -match '^\s*Stack Trace:\s*$')              { $state = 'stack'; continue }
        if ($line -match '^\s*Standard Output Messages:\s*$') { $state = 'other'; continue }
        if ($state -eq 'msg'   -and $line.Trim()) { $current.Message += $line.Trim() }
        elseif ($state -eq 'stack' -and $line.Trim()) { $current.Stack += $line.Trim() }
    }
    if ($current) { $failures += $current }

    if ($failures.Count -gt 0) {
        Write-Host ""
        Write-Host "  +-----------------------------+" -ForegroundColor DarkGray
        Write-Host "  |       Failure details       |" -ForegroundColor DarkGray
        Write-Host "  +-----------------------------+" -ForegroundColor DarkGray

        $groups = $failures | Group-Object { ($_.Message -join ' ').Trim() }
        foreach ($g in $groups) {
            $sample = $g.Group[0]
            Write-Host ""
            Write-Host ("  x {0} test(s) failed with:" -f $g.Count) -ForegroundColor Red

            foreach ($m in $sample.Message) {
                Write-Host "      $m" -ForegroundColor Yellow
            }

            $ownFrames = $sample.Stack | Where-Object { $_ -match 'Concertable' } | Select-Object -First 8
            if (-not $ownFrames) { $ownFrames = $sample.Stack | Select-Object -First 3 }
            foreach ($f in $ownFrames) {
                Write-Host ("      {0}" -f ($f -replace [regex]::Escape("$repoRoot\"), '')) -ForegroundColor DarkGray
            }

            $names = $g.Group.Test
            $shown = $names | Select-Object -First 3
            $rest  = $names.Count - $shown.Count
            $label = ($shown -join ', ')
            if ($rest -gt 0) { $label += " (+$rest more)" }
            Write-Host "      affected: $label" -ForegroundColor DarkGray
        }
    }

    $summaryLine = $lines | Where-Object { $_ -match '^\s*(Passed|Failed)!\s+-\s+Failed:\s+\d+' } | Select-Object -Last 1

    if ($summaryLine -match 'Failed:\s+(\d+),\s+Passed:\s+(\d+),\s+Skipped:\s+(\d+),\s+Total:\s+(\d+)') {
        $failed  = [int]$Matches[1]
        $passed  = [int]$Matches[2]
        $skipped = [int]$Matches[3]
        $total   = [int]$Matches[4]

        $passedColor  = if ($passed -gt 0)  { 'Green' }  else { 'DarkGray' }
        $failedColor  = if ($failed -gt 0)  { 'Red' }    else { 'DarkGray' }
        $skippedColor = if ($skipped -gt 0) { 'Yellow' } else { 'DarkGray' }

        Write-Host ""
        Write-Host "  +-----------------------------+" -ForegroundColor DarkGray
        Write-Host "  |       Test Results          |" -ForegroundColor DarkGray
        Write-Host "  +-----------------------------+" -ForegroundColor DarkGray
        Write-Host "  |  Passed  : " -NoNewline -ForegroundColor DarkGray
        Write-Host ("$passed".PadRight(17)) -NoNewline -ForegroundColor $passedColor
        Write-Host "|" -ForegroundColor DarkGray
        Write-Host "  |  Failed  : " -NoNewline -ForegroundColor DarkGray
        Write-Host ("$failed".PadRight(17)) -NoNewline -ForegroundColor $failedColor
        Write-Host "|" -ForegroundColor DarkGray
        Write-Host "  |  Skipped : " -NoNewline -ForegroundColor DarkGray
        Write-Host ("$skipped".PadRight(17)) -NoNewline -ForegroundColor $skippedColor
        Write-Host "|" -ForegroundColor DarkGray
        Write-Host "  |  Total   : " -NoNewline -ForegroundColor DarkGray
        Write-Host ("$total".PadRight(17)) -NoNewline -ForegroundColor White
        Write-Host "|" -ForegroundColor DarkGray
        Write-Host "  +-----------------------------+" -ForegroundColor DarkGray

        if ($failed -eq 0) {
            Write-Host "  |  All tests passed!          |" -ForegroundColor Green
        } else {
            Write-Host ("  |  $failed test(s) failed".PadRight(33) + "|") -ForegroundColor Red
        }

        Write-Host "  +-----------------------------+" -ForegroundColor DarkGray
        Write-Host ""
    }

    Write-Host "  Full log: $logFile" -ForegroundColor DarkGray
    Write-Host ""
}
catch {
    Write-Host "  Script error: $_" -ForegroundColor Red
}

exit $exitCode
