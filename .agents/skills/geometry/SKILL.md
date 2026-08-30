---
name: geometry
description: Concertable creates WGS84 geometry through the keyed `IGeometryProvider` rather than constructing NetTopologySuite types directly, because the provider owns the SRID and a hand-built geometry silently carries the wrong coordinate system. Use when storing or querying a coordinate here, or when reviewing a `new Point(...)` or `new GeometryFactory()` call.
---

# Geometry — create WGS84 points through `IGeometryProvider`

Inject the keyed provider and call it:

```csharp
[FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider
// geometryProvider.CreatePoint(latitude, longitude)
```

Never `new GeometryFactory()` or `new Point(...)`. The provider owns the SRID, so a hand-built geometry is
the one that silently carries the wrong coordinate system.
