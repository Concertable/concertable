$worktree = 'C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_deal-lifecycle-modules-phase2'
$promptPath = Join-Path $worktree 'scratchpad\claude-pr633-workflow-review-prompt.md'
$prompt = Get-Content -Raw -LiteralPath $promptPath
Set-Location -LiteralPath $worktree
claude --permission-mode plan $prompt
