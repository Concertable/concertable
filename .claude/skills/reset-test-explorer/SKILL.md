---
name: reset-test-explorer
description: Reset VS Test Explorer for the Concertable solution — cleans bin/obj for all Reqnroll UI E2E test projects, deletes the .vs folder so VS rediscovers everything from scratch, then tells the user what to do next. Use whenever VS Test Explorer is showing stale traits (FeatureTitle, TestType), missing tests, or wrong test counts.
---

# reset-test-explorer

Resets VS Test Explorer for the Concertable solution by forcing a clean rebuild of the Reqnroll UI
E2E projects and nuking VS's discovery cache.

## Why this is needed

VS caches test discovery in `api/.vs/Concertable.slnx/v18/TestStore/`. VS only registers a test
project as a container when IT builds the project — CLI-built DLLs are not automatically
registered as test containers. If VS considers a project "already up to date" (because it finds
bin/ populated), it may skip the build and never register it.

The `StripFeatureTraits` MSBuild task runs `BeforeTargets="CoreCompile"`, so VS's Build All
always correctly strips scenario-tag traits and injects `[Category("Ui")]` regardless of whether
Reqnroll regenerates the `.feature.cs` files (incremental or full build).

Symptoms that need this reset:
- `FeatureTitle [...]` or `TestType [Unit]` groups appearing in Test Explorer (stripped at build
  time; VS is showing stale cache)
- A project's tests missing entirely (e.g. B2B UI tests showing but not Customer, or vice versa)
- Wrong test counts after adding/removing scenarios

## Step 1 — Close Visual Studio

Tell the user: **"Close Visual Studio before I proceed."** Wait for confirmation.

## Step 2 — Full reset (bin/obj + VS cache)

Run all deletions in one shot — do NOT CLI pre-build afterwards:

```powershell
Remove-Item -Recurse -Force "api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests.Ui/bin" -Confirm:$false -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force "api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests.Ui/obj" -Confirm:$false -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force "api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests.Ui/bin" -Confirm:$false -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force "api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests.Ui/obj" -Confirm:$false -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force "api/.vs" -Confirm:$false -ErrorAction SilentlyContinue
Write-Host "Done"
```

VS must build the Ui projects itself to register them as test containers. Never CLI pre-build them.

## Step 3 — Tell the user what to do next

Tell the user:

> **Open Visual Studio, then press `Ctrl+Shift+B` (Build All) before checking Test Explorer.**
> Wait for the build to fully finish before opening Test Explorer.

After Build All completes, Test Explorer should show:
- `Category [Ui] (30)` — 23 B2B + 7 Customer tests
- `Category [Integration] (130+)`
- `Category [Unit]`
- `Category [Api]`

No `FeatureTitle`, `TestType`, or scenario-tag-derived `Category` groups.

## Notes

- Only the Reqnroll UI E2E projects need bin/obj cleaned — other test projects (integration, unit)
  are already tracked by VS because they were built in a previous session.
- The `StripFeatureTraits` MSBuild task in `api/Tests/Directory.Build.targets` runs
  `BeforeTargets="CoreCompile"` — it always fires before compilation, stripping Reqnroll's
  auto-generated scenario-tag traits and injecting `[Trait("Category", "Ui")]` on every test
  method. This holds for both incremental and full builds in VS.
- To sub-group inside `Category [Ui]` by project (B2B vs Customer): in Test Explorer click
  **Group By → add Project** as a secondary level after Trait.
