# Scaffolding only builds the EF model - it never opens the connection - so these need only be parseable,
# never real credentials (no user/password). Applying migrations to a real DB is a separate Aspire job
# that resolves the live string from config/Key Vault; it never runs this script.
$env:ConnectionStrings__B2BDb = "Server=localhost;Database=concertable-b2b;Trusted_Connection=True;TrustServerCertificate=True"
$env:ConnectionStrings__CustomerDb = "Server=localhost;Database=concertable-customer;Trusted_Connection=True;TrustServerCertificate=True"
$env:ConnectionStrings__PaymentDb = "Server=localhost;Database=concertable-payment;Trusted_Connection=True;TrustServerCertificate=True"
$env:ConnectionStrings__SearchDb = "Server=localhost;Database=concertable-search;Trusted_Connection=True;TrustServerCertificate=True"

# A module's migration content is compared old-vs-new with its 14-digit id normalized out (filenames
# and the Designer.cs [Migration("...")] attribute are the only places it appears). When a module's
# model didn't change, the regenerated migration is byte-identical modulo that id, so the old
# (already-published-package-matching) id is kept instead of re-stamping it - this is what stops a
# packaged lib (Messaging, Payment, Auth) from drifting its source migration id away from the id
# baked into its published NuGet package, which otherwise collides two different migration ids
# creating the same table against one DB ("There is already an object named 'Outbox'").
function Get-NormalizedMigrationFiles([string]$Dir) {
    $files = @{}
    if (-not (Test-Path $Dir)) { return $files }
    Get-ChildItem -File $Dir | ForEach-Object {
        $key = $_.Name -replace '^\d{14}_', ''
        $content = (Get-Content -Raw $_.FullName) -replace '\d{14}(?=_InitialCreate)', 'TIMESTAMP'
        $files[$key] = $content
    }
    return $files
}

function Test-MigrationUnchanged([string]$OldDir, [string]$NewDir) {
    $old = Get-NormalizedMigrationFiles $OldDir
    $new = Get-NormalizedMigrationFiles $NewDir
    if ($old.Count -eq 0 -or $old.Count -ne $new.Count) { return $false }
    foreach ($key in $old.Keys) {
        if (-not $new.ContainsKey($key)) { return $false }
        if ($old[$key] -ne $new[$key]) { return $false }
    }
    return $true
}

function Invoke-ScaffoldIfChanged {
    param(
        [string]$Context,
        [string]$Project,
        [string]$StartupProject,
        [string]$OutputDir
    )

    $dir = Join-Path $Project $OutputDir
    $backup = $null
    if (Test-Path $dir) {
        $backup = Join-Path ([System.IO.Path]::GetTempPath()) "initial-migrations-backup-$([guid]::NewGuid())"
        Copy-Item -Recurse -Force $dir $backup
        Remove-Item -Recurse -Force $dir
    }

    dotnet ef migrations add InitialCreate --context $Context --project $Project --startup-project $StartupProject --output-dir $OutputDir
    if ($LASTEXITCODE -ne 0) {
        if ($backup) {
            Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $dir
            Move-Item $backup $dir
        }
        exit 1
    }

    if ($backup) {
        if (Test-MigrationUnchanged -OldDir $backup -NewDir $dir) {
            Remove-Item -Recurse -Force $dir
            Move-Item $backup $dir
            Write-Host "  $Context unchanged - kept existing migration id"
        } else {
            Remove-Item -Recurse -Force $backup
        }
    }
}

# Messaging is consumed everywhere as a published package, so a service host can't be its startup
# project (EF would load the packaged assembly, which already contains InitialCreate). It scaffolds
# standalone via its design-time factories.
Invoke-ScaffoldIfChanged -Context OutboxDbContext -Project Concertable.Messaging/Concertable.Messaging.Infrastructure -StartupProject Concertable.Messaging/Concertable.Messaging.Infrastructure -OutputDir Data/Migrations/Outbox

Invoke-ScaffoldIfChanged -Context InboxDbContext -Project Concertable.Messaging/Concertable.Messaging.Infrastructure -StartupProject Concertable.Messaging/Concertable.Messaging.Infrastructure -OutputDir Data/Migrations/Inbox

Invoke-ScaffoldIfChanged -Context UserDbContext -Project Concertable.B2B/src/Modules/User/Concertable.B2B.User.Infrastructure -StartupProject Concertable.B2B/src/Concertable.B2B.Web -OutputDir Data/Migrations

Invoke-ScaffoldIfChanged -Context TenantDbContext -Project Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Infrastructure -StartupProject Concertable.B2B/src/Concertable.B2B.Web -OutputDir Data/Migrations

Invoke-ScaffoldIfChanged -Context AdminDbContext -Project Concertable.B2B/src/Modules/Admin/Concertable.B2B.Admin.Infrastructure -StartupProject Concertable.B2B/src/Concertable.B2B.Web -OutputDir Data/Migrations

Invoke-ScaffoldIfChanged -Context PrivacyDbContext -Project Concertable.B2B/src/Modules/Privacy/Concertable.B2B.Privacy.Infrastructure -StartupProject Concertable.B2B/src/Concertable.B2B.Web -OutputDir Data/Migrations

Invoke-ScaffoldIfChanged -Context ArtistDbContext -Project Concertable.B2B/src/Modules/Artist/Concertable.B2B.Artist.Infrastructure -StartupProject Concertable.B2B/src/Concertable.B2B.Web -OutputDir Data/Migrations

Invoke-ScaffoldIfChanged -Context VenueDbContext -Project Concertable.B2B/src/Modules/Venue/Concertable.B2B.Venue.Infrastructure -StartupProject Concertable.B2B/src/Concertable.B2B.Web -OutputDir Data/Migrations

Invoke-ScaffoldIfChanged -Context ConcertDbContext -Project Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure -StartupProject Concertable.B2B/src/Concertable.B2B.Web -OutputDir Data/Migrations

Invoke-ScaffoldIfChanged -Context DealDbContext -Project Concertable.B2B/src/Modules/Deal/Concertable.B2B.Deal.Infrastructure -StartupProject Concertable.B2B/src/Concertable.B2B.Web -OutputDir Data/Migrations

Invoke-ScaffoldIfChanged -Context PaymentDbContext -Project Concertable.Payment/src/Concertable.Payment.Infrastructure -StartupProject Concertable.Payment/src/Concertable.Payment.Web -OutputDir Data/Migrations

Invoke-ScaffoldIfChanged -Context ConversationsDbContext -Project Concertable.B2B/src/Modules/Conversations/Concertable.B2B.Conversations.Infrastructure -StartupProject Concertable.B2B/src/Concertable.B2B.Web -OutputDir Data/Migrations

Invoke-ScaffoldIfChanged -Context PersistedGrantDbContext -Project Concertable.Auth/src/Concertable.Auth -StartupProject Concertable.Auth/src/Concertable.Auth -OutputDir Data/Migrations/Duende

Invoke-ScaffoldIfChanged -Context AuthDbContext -Project Concertable.Auth/src/Concertable.Auth -StartupProject Concertable.Auth/src/Concertable.Auth -OutputDir Data/Migrations/Auth

Invoke-ScaffoldIfChanged -Context ConcertDbContext -Project Concertable.Customer/src/Modules/Concert/Concertable.Customer.Concert.Infrastructure -StartupProject Concertable.Customer/src/Concertable.Customer.Web -OutputDir Data/Migrations

Invoke-ScaffoldIfChanged -Context TicketDbContext -Project Concertable.Customer/src/Modules/Ticket/Concertable.Customer.Ticket.Infrastructure -StartupProject Concertable.Customer/src/Concertable.Customer.Web -OutputDir Data/Migrations

Invoke-ScaffoldIfChanged -Context ReviewDbContext -Project Concertable.Customer/src/Modules/Review/Concertable.Customer.Review.Infrastructure -StartupProject Concertable.Customer/src/Concertable.Customer.Web -OutputDir Data/Migrations

Invoke-ScaffoldIfChanged -Context UserDbContext -Project Concertable.Customer/src/Modules/User/Concertable.Customer.User.Infrastructure -StartupProject Concertable.Customer/src/Concertable.Customer.Web -OutputDir Data/Migrations

Invoke-ScaffoldIfChanged -Context PreferenceDbContext -Project Concertable.Customer/src/Modules/Preference/Concertable.Customer.Preference.Infrastructure -StartupProject Concertable.Customer/src/Concertable.Customer.Web -OutputDir Data/Migrations

Invoke-ScaffoldIfChanged -Context SearchDbContext -Project Concertable.Search/src/Concertable.Search.Infrastructure -StartupProject Concertable.Search/src/Concertable.Search.Web -OutputDir Data/Migrations

Invoke-ScaffoldIfChanged -Context VenueDbContext -Project Concertable.Customer/src/Modules/Venue/Concertable.Customer.Venue.Infrastructure -StartupProject Concertable.Customer/src/Concertable.Customer.Web -OutputDir Data/Migrations

Invoke-ScaffoldIfChanged -Context ArtistDbContext -Project Concertable.Customer/src/Modules/Artist/Concertable.Customer.Artist.Infrastructure -StartupProject Concertable.Customer/src/Concertable.Customer.Web -OutputDir Data/Migrations

Write-Host "All migrations scaffolded successfully."
