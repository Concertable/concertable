$dirs = @(
    "Concertable.Messaging\Concertable.Messaging.Infrastructure\Data\Migrations\Outbox",
    "Concertable.Messaging\Concertable.Messaging.Infrastructure\Data\Migrations\Inbox",
    "Concertable.Search\Concertable.Search.Infrastructure\Data\Migrations",
    "Concertable.B2B\Modules\User\Concertable.User.Infrastructure\Data\Migrations",
    "Concertable.B2B\Modules\Organization\Concertable.Organization.Infrastructure\Data\Migrations",
    "Concertable.B2B\Modules\Artist\Concertable.Artist.Infrastructure\Data\Migrations",
    "Concertable.B2B\Modules\Venue\Concertable.Venue.Infrastructure\Data\Migrations",
    "Concertable.B2B\Modules\Concert\Concertable.Concert.Infrastructure\Data\Migrations",
    "Concertable.B2B\Modules\Contract\Concertable.Contract.Infrastructure\Data\Migrations",
    "Concertable.Payment\Concertable.Payment.Infrastructure\Data\Migrations",
    "Concertable.B2B\Modules\Conversations\Concertable.Conversations.Infrastructure\Data\Migrations",
    "Concertable.Customer\Modules\Preference\Concertable.Customer.Preference.Infrastructure\Data\Migrations",
    "Concertable.Auth\Data\Migrations",
    "Concertable.Customer\Modules\Concert\Concertable.Customer.Concert.Infrastructure\Data\Migrations",
    "Concertable.Customer\Modules\Ticket\Concertable.Customer.Ticket.Infrastructure\Data\Migrations",
    "Concertable.Customer\Modules\Review\Concertable.Customer.Review.Infrastructure\Data\Migrations",
    "Concertable.Customer\Modules\User\Concertable.Customer.User.Infrastructure\Data\Migrations",
    "Concertable.Customer\Modules\Venue\Concertable.Customer.Venue.Infrastructure\Data\Migrations",
    "Concertable.Customer\Modules\Artist\Concertable.Customer.Artist.Infrastructure\Data\Migrations"
)
foreach ($d in $dirs) { Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $d }

dotnet ef migrations add InitialCreate --context OutboxDbContext --project Concertable.Messaging/Concertable.Messaging.Infrastructure --startup-project Concertable.B2B/Concertable.B2B.Web --output-dir Data/Migrations/Outbox
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context InboxDbContext --project Concertable.Messaging/Concertable.Messaging.Infrastructure --startup-project Concertable.B2B/Concertable.B2B.Web --output-dir Data/Migrations/Inbox
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context UserDbContext --project Concertable.B2B/Modules/User/Concertable.User.Infrastructure --startup-project Concertable.B2B/Concertable.B2B.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context OrganizationDbContext --project Concertable.B2B/Modules/Organization/Concertable.Organization.Infrastructure --startup-project Concertable.B2B/Concertable.B2B.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ArtistDbContext --project Concertable.B2B/Modules/Artist/Concertable.Artist.Infrastructure --startup-project Concertable.B2B/Concertable.B2B.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context VenueDbContext --project Concertable.B2B/Modules/Venue/Concertable.Venue.Infrastructure --startup-project Concertable.B2B/Concertable.B2B.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ConcertDbContext --project Concertable.B2B/Modules/Concert/Concertable.Concert.Infrastructure --startup-project Concertable.B2B/Concertable.B2B.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ContractDbContext --project Concertable.B2B/Modules/Contract/Concertable.Contract.Infrastructure --startup-project Concertable.B2B/Concertable.B2B.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context PaymentDbContext --project Concertable.Payment/Concertable.Payment.Infrastructure --startup-project Concertable.Payment/Concertable.Payment.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ConversationsDbContext --project Concertable.B2B/Modules/Conversations/Concertable.Conversations.Infrastructure --startup-project Concertable.B2B/Concertable.B2B.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context PersistedGrantDbContext --project Concertable.Auth --startup-project Concertable.Auth --output-dir Data/Migrations/Duende
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context AuthDbContext --project Concertable.Auth --startup-project Concertable.Auth --output-dir Data/Migrations/Auth
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ConcertDbContext --project Concertable.Customer/Modules/Concert/Concertable.Customer.Concert.Infrastructure --startup-project Concertable.Customer/Concertable.Customer.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context TicketDbContext --project Concertable.Customer/Modules/Ticket/Concertable.Customer.Ticket.Infrastructure --startup-project Concertable.Customer/Concertable.Customer.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ReviewDbContext --project Concertable.Customer/Modules/Review/Concertable.Customer.Review.Infrastructure --startup-project Concertable.Customer/Concertable.Customer.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context UserDbContext --project Concertable.Customer/Modules/User/Concertable.Customer.User.Infrastructure --startup-project Concertable.Customer/Concertable.Customer.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context PreferenceDbContext --project Concertable.Customer/Modules/Preference/Concertable.Customer.Preference.Infrastructure --startup-project Concertable.Customer/Concertable.Customer.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context SearchDbContext --project Concertable.Search/Concertable.Search.Infrastructure --startup-project Concertable.Search/Concertable.Search.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context VenueDbContext --project Concertable.Customer/Modules/Venue/Concertable.Customer.Venue.Infrastructure --startup-project Concertable.Customer/Concertable.Customer.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ArtistDbContext --project Concertable.Customer/Modules/Artist/Concertable.Customer.Artist.Infrastructure --startup-project Concertable.Customer/Concertable.Customer.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "All migrations scaffolded successfully."
