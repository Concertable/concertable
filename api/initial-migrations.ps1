$dirs = @(
    "Concertable.Messaging\Concertable.Messaging.Infrastructure\Data\Migrations\Outbox",
    "Concertable.Messaging\Concertable.Messaging.Infrastructure\Data\Migrations\Inbox",
    "Modules\Search\Concertable.Search.Infrastructure\Data\Migrations",
    "Modules\User\Concertable.User.Infrastructure\Data\Migrations",
    "Modules\Artist\Concertable.Artist.Infrastructure\Data\Migrations",
    "Modules\Venue\Concertable.Venue.Infrastructure\Data\Migrations",
    "Modules\Concert\Concertable.Concert.Infrastructure\Data\Migrations",
    "Modules\Contract\Concertable.Contract.Infrastructure\Data\Migrations",
    "Concertable.Payment\Concertable.Payment.Infrastructure\Data\Migrations",
    "Modules\Customer\Concertable.Customer.Infrastructure\Data\Migrations",
    "Modules\Conversations\Concertable.Conversations.Infrastructure\Data\Migrations",
    "Concertable.Auth\Data\Migrations",
    "Concertable.Customer\Modules\Concert\Concertable.Customer.Concert.Infrastructure\Data\Migrations",
    "Concertable.Customer\Modules\Ticket\Concertable.Customer.Ticket.Infrastructure\Data\Migrations",
    "Concertable.Customer\Modules\Review\Concertable.Customer.Review.Infrastructure\Data\Migrations",
    "Concertable.Customer\Modules\Profile\Concertable.Customer.Profile.Infrastructure\Data\Migrations"
)
foreach ($d in $dirs) { Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $d }

dotnet ef migrations add InitialCreate --context OutboxDbContext --project Concertable.Messaging/Concertable.Messaging.Infrastructure --startup-project Concertable.Web --output-dir Data/Migrations/Outbox
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context InboxDbContext --project Concertable.Messaging/Concertable.Messaging.Infrastructure --startup-project Concertable.Web --output-dir Data/Migrations/Inbox
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context UserDbContext --project Modules/User/Concertable.User.Infrastructure --startup-project Concertable.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ArtistDbContext --project Modules/Artist/Concertable.Artist.Infrastructure --startup-project Concertable.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context VenueDbContext --project Modules/Venue/Concertable.Venue.Infrastructure --startup-project Concertable.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ConcertDbContext --project Modules/Concert/Concertable.Concert.Infrastructure --startup-project Concertable.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ContractDbContext --project Modules/Contract/Concertable.Contract.Infrastructure --startup-project Concertable.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context PaymentDbContext --project Concertable.Payment/Concertable.Payment.Infrastructure --startup-project Concertable.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context CustomerDbContext --project Modules/Customer/Concertable.Customer.Infrastructure --startup-project Concertable.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ConversationsDbContext --project Modules/Conversations/Concertable.Conversations.Infrastructure --startup-project Concertable.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context PersistedGrantDbContext --project Concertable.Auth --startup-project Concertable.Auth --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ConcertDbContext --project Concertable.Customer/Modules/Concert/Concertable.Customer.Concert.Infrastructure --startup-project Concertable.Customer/Concertable.Customer.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context TicketDbContext --project Concertable.Customer/Modules/Ticket/Concertable.Customer.Ticket.Infrastructure --startup-project Concertable.Customer/Concertable.Customer.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ReviewDbContext --project Concertable.Customer/Modules/Review/Concertable.Customer.Review.Infrastructure --startup-project Concertable.Customer/Concertable.Customer.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context ProfileDbContext --project Concertable.Customer/Modules/Profile/Concertable.Customer.Profile.Infrastructure --startup-project Concertable.Customer/Concertable.Customer.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet ef migrations add InitialCreate --context SearchDbContext --project Modules/Search/Concertable.Search.Infrastructure --startup-project Concertable.Search/Concertable.Search.Web --output-dir Data/Migrations
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "All migrations scaffolded successfully."
