param(
    [switch]$SuccessOnly,
    [switch]$Headless
)

$project = "$PSScriptRoot\Concertable.E2ETests.Ui.csproj"
$filter  = if ($SuccessOnly) { "DisplayName~completes 3DS challenge" } else { "DisplayName~3DS" }

if ($Headless) { $env:HEADLESS = "true" }

try {
    dotnet test $project --filter $filter
} finally {
    if ($Headless) { Remove-Item Env:HEADLESS -ErrorAction SilentlyContinue }
}
