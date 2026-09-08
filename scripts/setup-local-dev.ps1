<# Compatibility router. Standalone scripts are independently runnable from each service root. #>
[CmdletBinding(SupportsShouldProcess)]
param(
    [ValidateSet('All', 'Auth', 'B2B', 'Customer', 'Payment', 'Search', 'FullStack', 'System')][string]$Owner = 'All',
    [string]$AppHostProject
)
$ErrorActionPreference = 'Stop'
if ($Owner -eq 'System') {
    if (-not $AppHostProject) { throw 'System requires -AppHostProject pointing to a container-only AppHost.' }
    & (Join-Path $PSScriptRoot 'system/setup-local-dev.ps1') -AppHostProject $AppHostProject -WhatIf:$WhatIfPreference
    return
}
if ($AppHostProject) { throw 'AppHostProject is only supported with -Owner System.' }
$apiRoot = Join-Path (Split-Path $PSScriptRoot -Parent) 'api'
$owners = if ($Owner -in @('All', 'FullStack')) { @('Auth', 'B2B', 'Customer', 'Payment', 'Search') } else { @($Owner) }
foreach ($item in $owners) {
    & (Join-Path $apiRoot "Concertable.$item/setup-local-dev.ps1") -WhatIf:$WhatIfPreference
}
if ($Owner -in @('All', 'FullStack')) {
    & (Join-Path $apiRoot 'Concertable.AppHost/setup-local-dev.ps1') -WhatIf:$WhatIfPreference
}
