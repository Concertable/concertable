<#
.SYNOPSIS
Idempotently registers this repo's agent-standards plugin marketplaces and plugins for Codex CLI.

.DESCRIPTION
Codex has no repo-committable equivalent of Claude Code's project-scoped plugin declaration
(.claude/settings.json's extraKnownMarketplaces/enabledPlugins) — plugin config is user-scoped only
(tracked upstream: https://github.com/openai/codex/issues/18115). Until that lands, every developer
running Codex against this repo runs this script once so skill_router.py can resolve the same
dotnet:*/dotnet-standards:*/react:*/react-standards:* skills AGENTS.md routes to, instead of reporting
every routed skill as "NOT INSTALLED". Safe to re-run: each add/install step is skipped when already
present.
#>

$ErrorActionPreference = 'Stop'

if (-not (Get-Command codex -ErrorAction SilentlyContinue)) {
    Write-Host "codex CLI not found on PATH — nothing to set up. Install Codex CLI first, then re-run this script." -ForegroundColor Yellow
    exit 0
}

$marketplaces = @(
    @{ Name = 'agent-standards'; Source = 'Concertable/agent-standards' },
    @{ Name = 'dotagents'; Source = 'tomjseery/dotagents' },
    @{ Name = 'react-agents'; Source = 'tomjseery/react-agents' }
)

$plugins = @(
    'agent-process@agent-standards',
    'dotnet@agent-standards',
    'react@agent-standards',
    'dotnet-standards@dotagents',
    'react-standards@react-agents'
)

$knownMarketplaces = (codex plugin marketplace list --json | ConvertFrom-Json).marketplaces.name
foreach ($marketplace in $marketplaces) {
    if ($knownMarketplaces -contains $marketplace.Name) {
        Write-Host "marketplace '$($marketplace.Name)' already registered" -ForegroundColor DarkGray
        continue
    }
    Write-Host "adding marketplace '$($marketplace.Name)' ($($marketplace.Source))..."
    codex plugin marketplace add $marketplace.Source
}

$installedPlugins = (codex plugin list --json | ConvertFrom-Json).installed | ForEach-Object { $_.pluginId }
foreach ($plugin in $plugins) {
    if ($installedPlugins -contains $plugin) {
        Write-Host "plugin '$plugin' already installed" -ForegroundColor DarkGray
        continue
    }
    Write-Host "installing plugin '$plugin'..."
    codex plugin add $plugin
}

Write-Host "Codex plugin setup complete." -ForegroundColor Green
