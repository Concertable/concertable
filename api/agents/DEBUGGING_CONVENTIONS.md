# Debugging Notes

- When a problem is hard to trace, add `ILogger<T>` to the relevant class and log key state — don't rely solely on exceptions or test output.
- Loggers should stay in production code permanently; they make the system more observable and are not debug-only scaffolding.
- Every log goes in the project's `Log.cs` as a `[LoggerMessage]` method — a one-off investigation probe included. `CA1848` is an error (`.editorconfig`), so an inline `logger.Log*` call does not compile. Delete the probe's `Log.cs` entry once the bug is fixed.
