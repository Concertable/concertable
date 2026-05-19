param(
    [switch]$SuccessOnly,
    [switch]$Headless
)

$project = "$PSScriptRoot\Concertable.E2ETests.Ui.csproj"
$filter  = if ($SuccessOnly) { "DisplayName~completes 3DS challenge" } else { "DisplayName~3DS" }

if ($Headless) { $env:HEADLESS = "true" }

$resultsDir = "$PSScriptRoot\TestResults"
$trxName    = "3ds-$(Get-Date -Format yyyyMMdd-HHmmss).trx"

try {
    dotnet test $project `
        --filter $filter `
        --logger "console;verbosity=minimal" `
        --logger "trx;LogFileName=$trxName" `
        --results-directory $resultsDir | Out-Null
    $exit = $LASTEXITCODE

    if ($exit -ne 0) {
        Write-Host ""
        Write-Host "=== Test run FAILED (exit $exit) ===" -ForegroundColor Red

        $trxPath = Join-Path $resultsDir $trxName
        if (Test-Path $trxPath) {
            [xml]$trx = Get-Content $trxPath
            $ns = New-Object Xml.XmlNamespaceManager $trx.NameTable
            $ns.AddNamespace('t', 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010')

            $failed = $trx.SelectNodes("//t:UnitTestResult[@outcome='Failed']", $ns)
            Write-Host "Failed tests: $($failed.Count)" -ForegroundColor Red

            foreach ($r in $failed) {
                Write-Host ""
                Write-Host "x $($r.testName)" -ForegroundColor Yellow

                $msg   = $r.SelectSingleNode('t:Output/t:ErrorInfo/t:Message',    $ns).InnerText
                $trace = $r.SelectSingleNode('t:Output/t:ErrorInfo/t:StackTrace', $ns).InnerText

                if ($msg) { Write-Host "  $($msg.Trim())" }

                if ($trace) {
                    $firstOwn = $trace -split "`n" |
                        Where-Object { $_ -match 'Concertable\.' } |
                        Select-Object -First 1
                    if ($firstOwn) { Write-Host "  at $($firstOwn.Trim())" -ForegroundColor DarkGray }
                }
            }

            Write-Host ""
            Write-Host "Full TRX: $trxPath" -ForegroundColor DarkGray
        } else {
            Write-Host "No TRX produced — failure was likely before tests ran (build/discovery)." -ForegroundColor Yellow
        }

        exit $exit
    }
} finally {
    if ($Headless) { Remove-Item Env:HEADLESS -ErrorAction SilentlyContinue }
}
