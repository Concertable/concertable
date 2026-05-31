# VS Test Explorer Troubleshooting

## Stale traits showing (FeatureTitle, TestType, unwanted Category groups)

The `StripFeatureTraits` MSBuild task strips Reqnroll-generated `[Trait]` attributes at build time,
but VS caches discovery results in `.vs/Concertable.slnx/v18/TestStore/`. If the cache is stale,
old traits persist in Test Explorer even after rebuilding.

**Fix:** close VS, delete the TestStore folder, reopen.

```powershell
Remove-Item -Recurse -Force "api/.vs/Concertable.slnx/v18/TestStore"
```

## Tests from a project are missing entirely

VS registers test containers during its own build. If a project's bin/obj was cleaned or the project
is new (untracked), VS may not have scanned its output DLL.

**Fix:** close VS, clean the affected project's output, rebuild from CLI so there's a fresh DLL,
then delete the TestStore and reopen.

```powershell
Remove-Item -Recurse -Force "api/Tests/<ProjectName>/bin"
Remove-Item -Recurse -Force "api/Tests/<ProjectName>/obj"
dotnet build "api/Tests/<ProjectName>/<ProjectName>.csproj"
Remove-Item -Recurse -Force "api/.vs/Concertable.slnx/v18/TestStore"
```

## Nothing above works / large-scale discovery problems

The entire `.vs` folder caches design-time build results, project hierarchy, and file indexes.
Deleting it forces VS to redo everything from scratch.

**Fix:** close VS, delete `api/.vs/`, reopen, then do **Build All** (`Ctrl+Shift+B`) before
checking Test Explorer. VS needs to build the projects itself to register all test containers.

```powershell
Remove-Item -Recurse -Force "api/.vs"
```

## How test traits work in this repo

- Assembly-level `Category` traits (e.g. `Category=Ui`, `Category=Integration`) come from
  `AssemblyInfo.cs` files in each test project — these are the intended groupings.
- Reqnroll generates `[Trait("FeatureTitle", ...)]` and `[Trait("Category", ...)]` (from `@tags`)
  on each test class/method. The `StripFeatureTraits` MSBuild task in `Directory.Build.targets`
  strips these at build time so they don't pollute Test Explorer.
- If FeatureTitle/tag-derived Category groups reappear, it means the strip task didn't run
  (feature files unchanged so Reqnroll skipped regeneration) or the TestStore is stale. A clean
  rebuild or TestStore delete resolves it.
