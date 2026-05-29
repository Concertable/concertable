# Debugging Notes

- When a problem is hard to trace, add `ILogger<T>` to the relevant class and log key state — don't rely solely on exceptions or test output.
- Loggers should stay in production code permanently; they make the system more observable and are not debug-only scaffolding.
- If a log is generic and useful for the future (handler invoked/skipped/wrote, waiter progress, processor lifecycle), promote it to the project's `Log.cs` with `[LoggerMessage]` source-gen. If it's a one-off probe for the current investigation, leave it as an inline `logger.Log*` call and remove it once the bug is fixed.
