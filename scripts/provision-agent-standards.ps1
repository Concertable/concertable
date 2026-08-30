[CmdletBinding()]
param(
    [string] $Repository = (Get-Location).Path,
    [string] $AgentWorkboardRoot = 'C:\Users\TommySeery\source\repos\agent-workboard',
    [string] $CodexExecutable,
    [string] $CodexHome,
    [switch] $VerifyFreshCodexSession
)

$ErrorActionPreference = 'Stop'

$verification = Join-Path $PSScriptRoot 'verify-agent-standards.ps1'
& $verification `
    -RepositoryRoot $Repository `
    -AgentWorkboardRoot $AgentWorkboardRoot `
    -CodexExecutable $CodexExecutable `
    -CodexHome $CodexHome `
    -VerifyFreshCodexSession:$VerifyFreshCodexSession

if ($LASTEXITCODE -ne 0) {
    throw 'Repository-local Agent Standards verification failed.'
}
