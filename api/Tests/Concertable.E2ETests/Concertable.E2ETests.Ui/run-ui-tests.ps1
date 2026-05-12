param(
    [switch]$Headed
)

$env:HEADLESS = if ($Headed) { "false" } else { "true" }

dotnet test $PSScriptRoot/Concertable.E2ETests.Ui.csproj --logger "console;verbosity=normal"
exit $LASTEXITCODE
