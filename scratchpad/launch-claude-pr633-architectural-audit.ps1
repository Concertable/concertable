$worktree = 'C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_deal-lifecycle-modules-phase2'
$promptPath = Join-Path $worktree 'scratchpad\claude-pr633-architectural-value-audit.md'
$prompt = Get-Content -Raw -LiteralPath $promptPath

Set-Location -LiteralPath $worktree
claude --permission-mode plan --model opus --effort max --name 'PR633 Architecture Value Audit' $prompt
