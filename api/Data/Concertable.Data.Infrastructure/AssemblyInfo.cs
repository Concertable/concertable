using System.Runtime.CompilerServices;

// Legacy Concertable.Infrastructure (GenreRepository) still needs to see SharedDbContext
// until the Genre module is extracted.
[assembly: InternalsVisibleTo("Concertable.Infrastructure")]
