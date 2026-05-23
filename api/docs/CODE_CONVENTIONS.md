# Code Conventions

## Private fields — no underscore prefix

Use `this.field` disambiguation in constructors instead of `_field` prefixes.

```csharp
// CORRECT
private readonly SearchDbContext context;

public MyService(SearchDbContext context)
{
    this.context = context;
}

// WRONG
private readonly SearchDbContext _context;

public MyService(SearchDbContext context)
{
    _context = context;
}
```

## No primary constructors for services

Services, repositories, handlers, and validators use an explicit constructor with `private readonly` fields assigned via `this.field = param`. No primary constructor shorthand.

## Single-statement branches — no braces

```csharp
// CORRECT
if (condition)
    return;

// WRONG
if (condition)
{
    return;
}
```

## No comments on WHAT the code does

Only add a comment when the WHY is non-obvious (hidden constraint, subtle invariant, workaround for a specific bug). Never narrate what the code does — well-named identifiers already do that.

## Geometry — use IGeometryProvider

Inject `[FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider` for WGS84 point creation. Never instantiate `GeometryFactory` or `new Point(...)` directly.

```csharp
var location = geometryProvider.CreatePoint(e.Latitude, e.Longitude);
```
