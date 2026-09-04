<# Compatibility delegator. Each owner keeps its model and migration IDs inside its own tree. #>
[CmdletBinding(SupportsShouldProcess)]
param(
    [ValidateSet('All', 'Auth', 'B2B', 'Customer', 'Payment', 'Search', 'Messaging')][string]$Owner = 'All',
    [string]$Context,
    [switch]$Check
)
$ErrorActionPreference = 'Stop'
if ($Owner -eq 'All' -and $Context) { throw 'Select one Owner when filtering by Context.' }
$owners = if ($Owner -eq 'All') { @('Messaging', 'Auth', 'B2B', 'Customer', 'Payment', 'Search') } else { @($Owner) }
foreach ($item in $owners) {
    & (Join-Path $PSScriptRoot "Concertable.$item/initial-migrations.ps1") -Context $Context -Check:$Check -WhatIf:$WhatIfPreference
}
