<# Replicate the platform-owned operations helper; -Check is the non-mutating parity gate. #>
[CmdletBinding(SupportsShouldProcess)]
param([switch]$Check)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$source = Join-Path $repoRoot 'api/Concertable.Shared/tools/OwnerOperations.psm1'
$copies = @('Auth', 'B2B', 'Customer', 'Payment', 'Search', 'Messaging', 'AppHost') |
    ForEach-Object { Join-Path $repoRoot "api/Concertable.$_/tools/OwnerOperations.psm1" }
$copies += Join-Path $PSScriptRoot 'system/OwnerOperations.psm1'
$expected = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
foreach ($copy in $copies) {
    if ((Test-Path -LiteralPath $copy) -and (Get-FileHash -LiteralPath $copy -Algorithm SHA256).Hash -eq $expected) { continue }
    if ($Check) { throw "Owner tooling differs from canonical platform source: $copy" }
    if ($PSCmdlet.ShouldProcess($copy, 'Refresh platform-owned tooling copy')) { Copy-Item -LiteralPath $source -Destination $copy }
}
Write-Host "Owner tooling parity verified: $($copies.Count) copies."
