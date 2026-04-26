$dirs = @(
    "Concertable.Data.Infrastructure\Data\Migrations",
    "Modules\Identity\Concertable.Identity.Infrastructure\Data\Migrations",
    "Modules\Artist\Concertable.Artist.Infrastructure\Data\Migrations",
    "Modules\Venue\Concertable.Venue.Infrastructure\Data\Migrations",
    "Modules\Concert\Concertable.Concert.Infrastructure\Data\Migrations",
    "Modules\Contract\Concertable.Contract.Infrastructure\Data\Migrations",
    "Modules\Payment\Concertable.Payment.Infrastructure\Data\Migrations",
    "Concertable.Infrastructure\Migrations"
)
foreach ($d in $dirs) { Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $d }

dotnet ef migrations add InitialCreate --context SharedDbContext --project Concertable.Data.Infrastructure --startup-project Concertable.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context IdentityDbContext --project Modules/Identity/Concertable.Identity.Infrastructure --startup-project Concertable.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ArtistDbContext --project Modules/Artist/Concertable.Artist.Infrastructure --startup-project Concertable.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context VenueDbContext --project Modules/Venue/Concertable.Venue.Infrastructure --startup-project Concertable.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ConcertDbContext --project Modules/Concert/Concertable.Concert.Infrastructure --startup-project Concertable.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ContractDbContext --project Modules/Contract/Concertable.Contract.Infrastructure --startup-project Concertable.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context PaymentDbContext --project Modules/Payment/Concertable.Payment.Infrastructure --startup-project Concertable.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ApplicationDbContext --project Concertable.Infrastructure --startup-project Concertable.Web --output-dir Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "All migrations scaffolded successfully."
