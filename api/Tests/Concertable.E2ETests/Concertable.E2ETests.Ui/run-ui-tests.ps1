param(
    [switch]$Headed
)

$env:HEADLESS = if ($Headed) { "false" } else { "true" }

$tempFile = [System.IO.Path]::GetTempFileName()

Write-Host ""
Write-Host "  Building..." -ForegroundColor DarkGray

try {
    dotnet test $PSScriptRoot/Concertable.E2ETests.Ui.csproj --logger "console;verbosity=normal" | ForEach-Object {
        Out-File -FilePath $tempFile -InputObject $_ -Append -Encoding utf8
        if ($_ -match '^\s+Passed ') {
            Write-Host $_ -ForegroundColor Green
        } elseif ($_ -match '^\s+Failed ') {
            Write-Host $_ -ForegroundColor Red
        } elseif ($_ -match 'Build succeeded') {
            Write-Host ""
            Write-Host "  Running tests..." -ForegroundColor DarkGray
            Write-Host ""
        }
    }
    $exitCode = $LASTEXITCODE

    $summaryLine = Get-Content $tempFile | Where-Object { $_ -match '^\s*(Passed|Failed)!\s+-\s+Failed:\s+\d+' } | Select-Object -Last 1

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
}
finally {
    Remove-Item $tempFile -ErrorAction SilentlyContinue
}

exit $exitCode
