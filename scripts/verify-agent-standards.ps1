[CmdletBinding()]
param(
    [string] $RepositoryRoot = (Get-Location).Path,
    [string] $AgentWorkboardRoot = 'C:\Users\TommySeery\source\repos\agent-workboard',
    [string] $CodexExecutable,
    [string] $CodexHome,
    [switch] $VerifyFreshCodexSession
)

$ErrorActionPreference = 'Stop'

function Assert-Condition([bool] $Condition, [string] $Message) {
    if (-not $Condition) {
        throw $Message
    }
}

function Get-RepositoryOrigin([string] $Root) {
    $origin = (& git -C $Root remote get-url origin).Trim()
    Assert-Condition ($LASTEXITCODE -eq 0) "Could not read the origin for $Root."
    return $origin
}

function Get-RoutedSkillNames([string] $Root) {
    $routes = Get-Content -Raw (Join-Path $Root '.agents\skill-routes.json') | ConvertFrom-Json
    return @($routes.routes | ForEach-Object { $_.skills } | Sort-Object -Unique)
}

function Assert-ConcertableDeployment([string] $Root) {
    $origin = Get-RepositoryOrigin $Root
    Assert-Condition ($origin -match '^(?:(?:https?|ssh)://github\.com/Concertable/[^/\s]+(?:\.git)?|git@github\.com:Concertable/[^/\s]+(?:\.git)?)$') "Repository is not a Concertable checkout: $origin"

    $skillsRoot = Join-Path $Root '.agents\skills'
    $routedSkills = Get-RoutedSkillNames $Root
    foreach ($skill in $routedSkills) {
        Assert-Condition (Test-Path -LiteralPath (Join-Path $skillsRoot "$skill\SKILL.md")) "Missing repository-local skill: $skill"
    }

    $standardReferences = @(
        Get-ChildItem -LiteralPath $skillsRoot -Recurse -File -Filter 'SKILL.md' | ForEach-Object {
            $content = Get-Content -Raw -LiteralPath $_.FullName
            [regex]::Matches($content, '(?<=\.\./\.\./standards/)[A-Za-z0-9_./-]+\.md') | ForEach-Object Value
        } | Sort-Object -Unique
    )
    foreach ($reference in $standardReferences) {
        Assert-Condition (Test-Path -LiteralPath (Join-Path $Root ".agents\standards\$reference")) "Missing repository-local standard referenced by a skill: $reference"
    }

    $agentFiles = @(Get-ChildItem -LiteralPath (Join-Path $Root '.codex\agents') -File -Filter 'workflow-*.toml')
    Assert-Condition ($agentFiles.Count -eq 5) "Expected five repository-local workflow agent files."

    $hooks = Get-Content -Raw (Join-Path $Root '.codex\hooks.json') | ConvertFrom-Json
    $sessionStart = @($hooks.hooks.SessionStart | ForEach-Object { $_.hooks } | Where-Object { $_.commandWindows -match 'session_floor\.py' })
    Assert-Condition ($sessionStart.Count -eq 1) 'Expected one repository-local Codex SessionStart floor hook.'
    Assert-Condition (Test-Path -LiteralPath (Join-Path $Root '.agents\hooks\session_floor.py')) 'Missing the repository-local SessionStart hook implementation.'

    Write-Host "Concertable deployment verified: $($routedSkills.Count) routed skills, $($standardReferences.Count) referenced standards, $($agentFiles.Count) workflow agents, and the SessionStart floor hook."
}

function Assert-AgentWorkboardIsolation([string] $Root) {
    Assert-Condition (Test-Path -LiteralPath $Root) "Agent Workboard checkout was not found: $Root"
    $forbidden = @(
        '.agents\skills',
        '.codex\agents',
        '.codex\hooks.json'
    )
    foreach ($relative in $forbidden) {
        Assert-Condition (-not (Test-Path -LiteralPath (Join-Path $Root $relative))) "Agent Workboard must not carry Agent Standards at $relative."
    }
    Write-Host 'Agent Workboard isolation verified: no repository-local Agent Standards assets found.'
}

function Get-CodexExecutable([string] $Configured) {
    if ($Configured) {
        Assert-Condition (Test-Path -LiteralPath $Configured) "Codex executable was not found: $Configured"
        return $Configured
    }

    $command = Get-Command codex -ErrorAction SilentlyContinue | Select-Object -First 1
    Assert-Condition ($null -ne $command) 'Codex CLI was not found. Pass -CodexExecutable to verify a fresh session.'
    return $command.Source
}

function Invoke-FreshSessionProbe([string] $Executable, [string] $CodexHomePath, [string] $Root, [string] $ExpectedPattern) {
    Assert-Condition (Test-Path -LiteralPath $CodexHomePath) "Codex home was not found: $CodexHomePath"
    $previousErrorActionPreference = $ErrorActionPreference
    $previousCodexHome = $env:CODEX_HOME
    $previousHome = $env:HOME
    $previousUserProfile = $env:USERPROFILE
    $ErrorActionPreference = 'Continue'
    try {
        $env:CODEX_HOME = $CodexHomePath
        $env:HOME = Split-Path -Parent $CodexHomePath
        $env:USERPROFILE = Split-Path -Parent $CodexHomePath
        $result = & $Executable exec --ephemeral --ignore-user-config --dangerously-bypass-hook-trust --color never --json -C $Root 'Invoke the floor skill before answering. Reply with the title of its SKILL.md.' 2>&1 | Out-String
        $exitCode = $LASTEXITCODE
    } finally {
        $ErrorActionPreference = $previousErrorActionPreference
        $env:CODEX_HOME = $previousCodexHome
        $env:HOME = $previousHome
        $env:USERPROFILE = $previousUserProfile
    }
    Assert-Condition ($exitCode -eq 0) "Fresh Codex session failed in ${Root}:`n$result"
    Assert-Condition ($result -match $ExpectedPattern) "Fresh Codex session did not match $ExpectedPattern in ${Root}:`n$result"
}

$repository = (Resolve-Path -LiteralPath $RepositoryRoot).Path
Assert-ConcertableDeployment $repository
Assert-AgentWorkboardIsolation $AgentWorkboardRoot

if ($VerifyFreshCodexSession) {
    $codex = Get-CodexExecutable $CodexExecutable
    Assert-Condition (-not [string]::IsNullOrWhiteSpace($CodexHome)) 'Pass -CodexHome when verifying a fresh Codex session.'
    Invoke-FreshSessionProbe $codex $CodexHome $repository '(?i)Behavioral Floor'
    Invoke-FreshSessionProbe $codex $CodexHome $AgentWorkboardRoot '(?i)floor.*(?:isn.t|not).*available'
    Write-Host 'Fresh Codex sessions verified with --ignore-user-config: Concertable resolves floor and Agent Workboard does not.'
}
