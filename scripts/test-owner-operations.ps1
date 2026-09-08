#requires -Version 7.0
# Offline behavior checks: no real SDK, secrets, database, or service process is invoked.
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repoRoot = Split-Path $PSScriptRoot -Parent
$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ("concertable-owner-tests-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporaryRoot | Out-Null
$global:OwnerTestCalls = [Collections.Generic.List[object]]::new()
$global:OwnerTestSecrets = @{}
$global:OwnerTestMode = 'Unexpected'
function Assert([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
function Assert-Throws([scriptblock]$Action, [string]$Pattern) {
    try { & $Action } catch {
        Assert ($_.Exception.Message -match $Pattern) "Unexpected failure: $($_.Exception.Message)"
        return
    }
    throw "Expected failure matching: $Pattern"
}
function Write-Fixture([string]$Path, [string]$Value) {
    New-Item -ItemType Directory -Path (Split-Path $Path -Parent) -Force | Out-Null
    [IO.File]::WriteAllText($Path, $Value)
}
function global:dotnet {
    $arguments = @($args)
    $global:OwnerTestCalls.Add($arguments)
    $global:LASTEXITCODE = 0
    if ($global:OwnerTestMode -eq 'Unexpected') { throw 'A dry run invoked dotnet.' }
    if ($arguments[0] -eq 'msbuild') {
        Assert ($arguments -contains '-getItem:ProjectReference') 'System must inspect evaluated references.'
        if ($global:OwnerTestMode -eq 'SystemFailure') { $global:LASTEXITCODE = 1; return }
        if ($global:OwnerTestMode -eq 'SystemReferences') {
            '{"Properties":{"IsAspireHost":"true"},"Items":{"ProjectReference":[{"Identity":"../runtime.csproj","DefiningProjectFullPath":"Directory.Build.targets"}]}}'
        } else {
            '{"Properties":{"IsAspireHost":"true"},"Items":{"ProjectReference":[]}}'
        }
        return
    }
    if ($arguments[0] -eq 'user-secrets') {
        if ($global:OwnerTestMode -eq 'SecretFailure') { $global:LASTEXITCODE = 1; return }
        if ($arguments[1] -eq 'list') {
            foreach ($key in $global:OwnerTestSecrets.Keys) { "$key = $($global:OwnerTestSecrets[$key])" }
        } else {
            $global:OwnerTestSecrets[$arguments[2]] = $arguments[3]
        }
        return
    }
    if ($arguments[2] -eq 'has-pending-model-changes') { return }
    $project = $arguments[[array]::IndexOf($arguments, '--project') + 1]
    $output = $arguments[[array]::IndexOf($arguments, '--output-dir') + 1]
    $directory = Join-Path $project $output
    Assert (@(Get-ChildItem -LiteralPath $project -Filter '*ModelSnapshot.cs' -Recurse -File).Count -eq 0) 'The EF project still contains backup snapshots.'
    $backups = @(Get-ChildItem -LiteralPath (Split-Path $project -Parent) -Filter '.migration-backup-*' -Directory -Force)
    Assert ($backups.Count -eq 1) 'Backup must be an immediate child of the owner root, independent of OS temp.'
    Assert ([IO.Path]::GetPathRoot($backups[0].FullName) -eq [IO.Path]::GetPathRoot($project)) 'Backup must remain on the project volume.'
    Write-Fixture (Join-Path $directory '20990101000000_InitialCreate.cs') 'model'
    Write-Fixture (Join-Path $directory '20990101000000_InitialCreate.Designer.cs') '[Migration("20990101000000_InitialCreate")]'
    Write-Fixture (Join-Path $directory 'TestDbContextModelSnapshot.cs') $(if ($global:OwnerTestMode -eq 'Changed') { 'changed' } else { 'snapshot' })
    if ($global:OwnerTestMode -eq 'ScaffoldFailure') { $global:LASTEXITCODE = 1 }
}
try {
    & (Join-Path $PSScriptRoot 'sync-owner-tooling.ps1') -Check
    $owners = @('Auth', 'B2B', 'Customer', 'Payment', 'Search', 'Messaging')
    $snapshotCount = 0
    foreach ($owner in $owners) {
        $source = Join-Path $repoRoot "api/Concertable.$owner"
        $manifest = Import-PowerShellDataFile (Join-Path $source 'migrations.psd1')
        $carve = Join-Path $temporaryRoot $owner
        foreach ($relative in @('initial-migrations.ps1', 'migrations.psd1', 'tools/OwnerOperations.psm1')) {
            $destination = Join-Path $carve $relative
            New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force | Out-Null
            Copy-Item -LiteralPath (Join-Path $source $relative) -Destination $destination
        }
        foreach ($migration in $manifest.Migrations) {
            $snapshot = Join-Path $source "$($migration.Project)/$($migration.OutputDir)/$($migration.Context)ModelSnapshot.cs"
            Assert (Test-Path -LiteralPath $snapshot) "Manifest has no matching snapshot: $snapshot"
            $snapshotCount++
            foreach ($project in @($migration.Project, $migration.StartupProject)) {
                New-Item -ItemType Directory -Path (Join-Path $carve $project) -Force | Out-Null
            }
        }
        & (Join-Path $carve 'initial-migrations.ps1') -WhatIf
        & (Join-Path $carve 'initial-migrations.ps1') -Check -WhatIf
        Assert-Throws { & (Join-Path $carve 'initial-migrations.ps1') -Context Missing -WhatIf } 'Unknown migration context'
        if ($owner -ne 'Messaging') {
            $appHost = "src/Concertable.$owner.AppHost/Concertable.$owner.AppHost.csproj"
            Write-Fixture (Join-Path $carve $appHost) (Get-Content -LiteralPath (Join-Path $source $appHost) -Raw)
            Copy-Item -LiteralPath (Join-Path $source 'setup-local-dev.ps1') -Destination $carve
            foreach ($template in Get-ChildItem -LiteralPath $source -Filter 'appsettings.Development.json.example' -Recurse -File) {
                $relative = [IO.Path]::GetRelativePath($source, $template.FullName)
                Write-Fixture (Join-Path $carve $relative) (Get-Content -LiteralPath $template.FullName -Raw)
            }
            & (Join-Path $carve 'setup-local-dev.ps1') -WhatIf
            Assert (@(Get-ChildItem -LiteralPath $carve -Filter appsettings.Development.json -Recurse).Count -eq 0) 'WhatIf wrote settings.'
        }
    }
    Assert ($snapshotCount -eq 24) 'Migration inventory changed; reconcile all owner manifests.'
    $auth = Import-PowerShellDataFile (Join-Path $repoRoot 'api/Concertable.Auth/migrations.psd1')
    Assert ($auth.Environment.Keys.Count -eq 1 -and $auth.Environment.ContainsKey('ConnectionStrings__AuthDb')) 'Auth must own only AuthDb.'
    & (Join-Path $repoRoot 'api/initial-migrations.ps1') -WhatIf
    & (Join-Path $repoRoot 'scripts/setup-local-dev.ps1') -WhatIf
    Assert ($global:OwnerTestCalls.Count -eq 0) 'Dry runs invoked dotnet.'
    $global:OwnerTestMode = 'SystemReferences'
    Assert-Throws { & (Join-Path $PSScriptRoot 'system/setup-local-dev.ps1') -AppHostProject (Join-Path $repoRoot 'api/Concertable.AppHost/Concertable.AppHost.csproj') -WhatIf } 'without source ProjectReferences'
    $systemProject = Join-Path $temporaryRoot 'system/Host.csproj'
    Write-Fixture $systemProject '<Project><PropertyGroup><IsAspireHost>true</IsAspireHost><UserSecretsId>offline-fixture</UserSecretsId></PropertyGroup></Project>'
    Assert-Throws { & (Join-Path $PSScriptRoot 'system/setup-local-dev.ps1') -AppHostProject $systemProject -WhatIf } 'without source ProjectReferences'
    $global:OwnerTestMode = 'SystemFailure'
    Assert-Throws { & (Join-Path $PSScriptRoot 'system/setup-local-dev.ps1') -AppHostProject $systemProject -WhatIf } 'Cannot evaluate System AppHost'
    $global:OwnerTestMode = 'SystemValid'
    & (Join-Path $PSScriptRoot 'system/setup-local-dev.ps1') -AppHostProject $systemProject -WhatIf

    $global:OwnerTestMode = 'Secrets'
    $global:OwnerTestSecrets['ServiceAuth:B2BClientSecret'] = 'existing-user-value'
    $bootstrap = Join-Path $temporaryRoot 'Auth/setup-local-dev.ps1'
    & $bootstrap
    $settings = Join-Path $temporaryRoot 'Auth/src/Concertable.Auth/appsettings.Development.json'
    Write-Fixture $settings '{"keep":"user-settings"}'
    $before = $global:OwnerTestCalls.Count
    & $bootstrap
    Assert ($global:OwnerTestCalls.Count -eq $before + 1) 'Idempotent bootstrap should only list secrets.'
    Assert ($global:OwnerTestSecrets.Count -eq 3) 'Bootstrap did not set all missing keys.'
    Assert ($global:OwnerTestSecrets['ServiceAuth:B2BClientSecret'] -eq 'existing-user-value') 'Bootstrap overwrote a secret.'
    Assert ((Get-Content -LiteralPath $settings -Raw) -eq '{"keep":"user-settings"}') 'Bootstrap overwrote settings.'
    $global:OwnerTestMode = 'SecretFailure'
    Assert-Throws { & $bootstrap } 'Cannot read AppHost user-secrets'

    Import-Module (Join-Path $repoRoot 'api/Concertable.Shared/tools/OwnerOperations.psm1') -Force
    $fixture = Join-Path $temporaryRoot 'migrations'
    $directory = Join-Path $fixture 'project/Data/Migrations'
    Write-Fixture (Join-Path $directory '20000101000000_InitialCreate.cs') 'model'
    Write-Fixture (Join-Path $directory '20000101000000_InitialCreate.Designer.cs') '[Migration("20000101000000_InitialCreate")]'
    Write-Fixture (Join-Path $directory 'TestDbContextModelSnapshot.cs') 'snapshot'
    $manifest = @{ Environment = @{ OWNER_TEST_CONNECTION = 'temporary' }; Migrations = @(@{
        Context = 'TestDbContext'; Project = 'project'; StartupProject = 'project'; OutputDir = 'Data/Migrations'
    }) }
    $saved = [Environment]::GetEnvironmentVariable('OWNER_TEST_CONNECTION', 'Process')
    [Environment]::SetEnvironmentVariable('OWNER_TEST_CONNECTION', 'original', 'Process')
    $global:OwnerTestMode = 'Unchanged'
    Invoke-OwnerMigrations -Root $fixture -Manifest $manifest
    Assert (Test-Path -LiteralPath (Join-Path $directory '20000101000000_InitialCreate.cs')) 'Unchanged migration ID was replaced.'
    Assert ($env:OWNER_TEST_CONNECTION -eq 'original') 'Caller environment was not restored.'
    $global:OwnerTestMode = 'ScaffoldFailure'
    Assert-Throws { Invoke-OwnerMigrations -Root $fixture -Manifest $manifest } 'Scaffolding failed'
    Assert (Test-Path -LiteralPath (Join-Path $directory '20000101000000_InitialCreate.cs')) 'Failed scaffold did not restore old files.'
    Assert ($env:OWNER_TEST_CONNECTION -eq 'original') 'Failure leaked scaffolding environment.'
    $global:OwnerTestMode = 'Changed'
    Invoke-OwnerMigrations -Root $fixture -Manifest $manifest
    Assert (Test-Path -LiteralPath (Join-Path $directory '20990101000000_InitialCreate.cs')) 'Changed model did not retain new migration.'
    Assert (@(Get-ChildItem -LiteralPath $fixture -Filter '.migration-backup-*' -Force).Count -eq 0) 'Backup was orphaned.'
    Invoke-OwnerMigrations -Root $fixture -Manifest $manifest -Check
    $manifest.Migrations[0].Project = '../escape'
    Assert-Throws { Invoke-OwnerMigrations -Root $fixture -Manifest $manifest -WhatIf } 'inside owner root'
    $external = Join-Path $temporaryRoot 'external-migrations'
    $externalMarker = Join-Path $external 'do-not-touch.txt'
    Write-Fixture $externalMarker 'preserved'
    $linkedProject = Join-Path $fixture 'linked-project'
    $linkType = if ([IO.Path]::DirectorySeparatorChar -eq '\') { 'Junction' } else { 'SymbolicLink' }
    New-Item -ItemType $linkType -Path $linkedProject -Target $external | Out-Null
    $manifest.Migrations[0].Project = 'linked-project'
    Assert-Throws { Invoke-OwnerMigrations -Root $fixture -Manifest $manifest -WhatIf } 'cannot traverse a link'
    Assert ((Get-Content -LiteralPath $externalMarker -Raw) -eq 'preserved') 'Rejected link traversal changed external content.'
    Remove-Item -LiteralPath $linkedProject -Force
    $caseRoot = Join-Path $temporaryRoot 'caseowner'
    $caseSibling = Join-Path $temporaryRoot 'CASEOWNER'
    New-Item -ItemType Directory -Path (Join-Path $caseRoot 'project') -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $caseSibling 'project') -Force | Out-Null
    $ownerModule = Get-Module OwnerOperations | Where-Object { $_.Path -eq (Join-Path $repoRoot 'api/Concertable.Shared/tools/OwnerOperations.psm1') } | Select-Object -First 1
    if ([IO.Path]::DirectorySeparatorChar -eq '/') {
        Assert-Throws { & $ownerModule { param($Root) Resolve-OwnerPath $Root '../CASEOWNER/project' } $caseRoot } 'inside owner root'
    } else {
        $resolved = & $ownerModule { param($Root) Resolve-OwnerPath $Root '../CASEOWNER/project' } $caseRoot
        Assert ($resolved -eq (Join-Path $caseSibling 'project')) 'Windows containment must remain case-insensitive.'
    }
    [Environment]::SetEnvironmentVariable('OWNER_TEST_CONNECTION', $saved, 'Process')
    Write-Host 'PASS: 24 contexts, 6 isolated migration carves, 5 isolated bootstraps, root dry runs, System gate, idempotency, rollback, ID stability, environment restoration, lexical and link path boundaries.'
} finally {
    Remove-Item Function:\dotnet -ErrorAction SilentlyContinue
    Remove-Variable OwnerTestCalls, OwnerTestSecrets, OwnerTestMode -Scope Global -ErrorAction SilentlyContinue
    $resolvedTemporaryRoot = [IO.Path]::GetFullPath($temporaryRoot)
    $tempPrefix = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if (-not $resolvedTemporaryRoot.StartsWith($tempPrefix, [StringComparison]::OrdinalIgnoreCase) -or
        (Split-Path $resolvedTemporaryRoot -Leaf) -notlike 'concertable-owner-tests-*') { throw 'Unsafe temporary cleanup path.' }
    Remove-Item -LiteralPath $resolvedTemporaryRoot -Recurse -Force
}
