# Debugging Notes

- When a problem is hard to trace, add `ILogger<T>` to the relevant class and log key state — don't rely solely on exceptions or test output.
- Loggers should stay in production code permanently; they make the system more observable and are not debug-only scaffolding.
