<# Replicate the platform-owned operations helper; -Check is the non-mutating parity gate. #>
[CmdletBinding(SupportsShouldProcess)]
param([switch]$Check)
$ErrorActionPreference = 'Stop'
function Get-FileSha256([string]$Path) {
    $algorithm = [Security.Cryptography.SHA256]::Create()
    $stream = [IO.File]::OpenRead($Path)
    try {
        return (($algorithm.ComputeHash($stream) | ForEach-Object { $_.ToString('x2') }) -join '')
    } finally {
        $stream.Dispose()
        $algorithm.Dispose()
    }
}
$repoRoot = Split-Path $PSScriptRoot -Parent
$source = Join-Path $repoRoot 'api/Concertable.Shared/tools/OwnerOperations.psm1'
$copies = @('Auth', 'B2B', 'Customer', 'Payment', 'Search', 'Messaging', 'AppHost') |
    ForEach-Object { Join-Path $repoRoot "api/Concertable.$_/tools/OwnerOperations.psm1" }
$copies += Join-Path $PSScriptRoot 'system/OwnerOperations.psm1'
$expected = Get-FileSha256 $source
foreach ($copy in $copies) {
    if ((Test-Path -LiteralPath $copy) -and (Get-FileSha256 $copy) -eq $expected) { continue }
    if ($Check) { throw "Owner tooling differs from canonical platform source: $copy" }
    if ($PSCmdlet.ShouldProcess($copy, 'Refresh platform-owned tooling copy')) { Copy-Item -LiteralPath $source -Destination $copy }
}
Write-Host "Owner tooling parity verified: $($copies.Count) copies."
