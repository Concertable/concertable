$dirs = @(
    "Concertable.Messaging\Concertable.Messaging.Infrastructure\Data\Migrations\Outbox",
    "Concertable.Messaging\Concertable.Messaging.Infrastructure\Data\Migrations\Inbox",
    "Concertable.Search\Concertable.Search.Infrastructure\Data\Migrations",
    "Concertable.B2B\src\Modules\User\Concertable.B2B.User.Infrastructure\Data\Migrations",
    "Concertable.B2B\src\Modules\Tenant\Concertable.B2B.Tenant.Infrastructure\Data\Migrations",
    "Concertable.B2B\src\Modules\Artist\Concertable.B2B.Artist.Infrastructure\Data\Migrations",
    "Concertable.B2B\src\Modules\Venue\Concertable.B2B.Venue.Infrastructure\Data\Migrations",
    "Concertable.B2B\src\Modules\Concert\Concertable.B2B.Concert.Infrastructure\Data\Migrations",
    "Concertable.B2B\src\Modules\Contract\Concertable.B2B.Contract.Infrastructure\Data\Migrations",
    "Concertable.Payment\src\Concertable.Payment.Infrastructure\Data\Migrations",
    "Concertable.B2B\src\Modules\Conversations\Concertable.B2B.Conversations.Infrastructure\Data\Migrations",
    "Concertable.Customer\src\Modules\Preference\Concertable.Customer.Preference.Infrastructure\Data\Migrations",
    "Concertable.Auth\Data\Migrations",
    "Concertable.Customer\src\Modules\Concert\Concertable.Customer.Concert.Infrastructure\Data\Migrations",
    "Concertable.Customer\src\Modules\Ticket\Concertable.Customer.Ticket.Infrastructure\Data\Migrations",
    "Concertable.Customer\src\Modules\Review\Concertable.Customer.Review.Infrastructure\Data\Migrations",
    "Concertable.Customer\src\Modules\User\Concertable.Customer.User.Infrastructure\Data\Migrations",
    "Concertable.Customer\src\Modules\Venue\Concertable.Customer.Venue.Infrastructure\Data\Migrations",
    "Concertable.Customer\src\Modules\Artist\Concertable.Customer.Artist.Infrastructure\Data\Migrations"
)
foreach ($d in $dirs) { Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $d }

dotnet ef migrations add InitialCreate --context OutboxDbContext --project Concertable.Messaging/Concertable.Messaging.Infrastructure --startup-project Concertable.B2B/Concertable.B2B.Web --output-dir Data/Migrations/Outbox
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context InboxDbContext --project Concertable.Messaging/Concertable.Messaging.Infrastructure --startup-project Concertable.B2B/Concertable.B2B.Web --output-dir Data/Migrations/Inbox
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context UserDbContext --project Concertable.B2B/src/Modules/User/Concertable.B2B.User.Infrastructure --startup-project Concertable.B2B/Concertable.B2B.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context TenantDbContext --project Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Infrastructure --startup-project Concertable.B2B/Concertable.B2B.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ArtistDbContext --project Concertable.B2B/src/Modules/Artist/Concertable.B2B.Artist.Infrastructure --startup-project Concertable.B2B/Concertable.B2B.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context VenueDbContext --project Concertable.B2B/src/Modules/Venue/Concertable.B2B.Venue.Infrastructure --startup-project Concertable.B2B/Concertable.B2B.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ConcertDbContext --project Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure --startup-project Concertable.B2B/Concertable.B2B.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ContractDbContext --project Concertable.B2B/src/Modules/Contract/Concertable.B2B.Contract.Infrastructure --startup-project Concertable.B2B/Concertable.B2B.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context PaymentDbContext --project Concertable.Payment/src/Concertable.Payment.Infrastructure --startup-project Concertable.Payment/src/Concertable.Payment.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ConversationsDbContext --project Concertable.B2B/src/Modules/Conversations/Concertable.B2B.Conversations.Infrastructure --startup-project Concertable.B2B/Concertable.B2B.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context PersistedGrantDbContext --project Concertable.Auth --startup-project Concertable.Auth --output-dir Data/Migrations/Duende
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context AuthDbContext --project Concertable.Auth --startup-project Concertable.Auth --output-dir Data/Migrations/Auth
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ConcertDbContext --project Concertable.Customer/src/Modules/Concert/Concertable.Customer.Concert.Infrastructure --startup-project Concertable.Customer/Concertable.Customer.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context TicketDbContext --project Concertable.Customer/src/Modules/Ticket/Concertable.Customer.Ticket.Infrastructure --startup-project Concertable.Customer/Concertable.Customer.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ReviewDbContext --project Concertable.Customer/src/Modules/Review/Concertable.Customer.Review.Infrastructure --startup-project Concertable.Customer/Concertable.Customer.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context UserDbContext --project Concertable.Customer/src/Modules/User/Concertable.Customer.User.Infrastructure --startup-project Concertable.Customer/Concertable.Customer.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context PreferenceDbContext --project Concertable.Customer/src/Modules/Preference/Concertable.Customer.Preference.Infrastructure --startup-project Concertable.Customer/Concertable.Customer.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context SearchDbContext --project Concertable.Search/Concertable.Search.Infrastructure --startup-project Concertable.Search/Concertable.Search.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context VenueDbContext --project Concertable.Customer/src/Modules/Venue/Concertable.Customer.Venue.Infrastructure --startup-project Concertable.Customer/Concertable.Customer.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ArtistDbContext --project Concertable.Customer/src/Modules/Artist/Concertable.Customer.Artist.Infrastructure --startup-project Concertable.Customer/Concertable.Customer.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "All migrations scaffolded successfully."
