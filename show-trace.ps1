$latest = Get-ChildItem -Recurse -Filter "trace-*.zip" "$PSScriptRoot/api/Tests/Concertable.E2ETests" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if (-not $latest) { Write-Error "No trace files found."; exit 1 }

Write-Host "Opening: $($latest.FullName)"
npx playwright show-trace $latest.FullName
