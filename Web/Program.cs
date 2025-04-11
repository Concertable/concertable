using Application.Mappings;
using Core.Entities.Identity;
using Application.Interfaces;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Data.Identity;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Web;
using Infrastructure.Settings;
using Application.DTOs;
using Core.Entities;
using Infrastructure.Factories;
using Web.Hubs;
using Application.Serializers;
using Microsoft.AspNetCore.Authorization;
using Web.Authorization;
using QuestPDF.Infrastructure;
using Infrastructure.Background;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Configuration.AddEnvironmentVariables();
Console.WriteLine("Stripe__SecretKey: " + Environment.GetEnvironmentVariable("Stripe__SecretKey")?.Substring(0, 8) ?? "NULL");

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.IncludeFields = true; //Include nested fields 

    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles; //Ignore cycles
    options.JsonSerializerOptions.WriteIndented = true; //JSON formatting
    options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
});
builder.Services.AddHttpContextAccessor();

builder.Services.AddLogging();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials()
               .WithOrigins("http://localhost:4200", "https://localhost:4200");
    });
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;

    options.SignIn.RequireConfirmedEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints();

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IListingApplicationService, ListingApplicationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IManagerService, ManagerService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IUserPaymentService, UserPaymentService>();
builder.Services.AddScoped<IStripeAccountService, StripeAccountService>();
builder.Services.AddScoped<IPreferenceService, PreferenceService>();
builder.Services.AddScoped<IUriService, UriService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IListingApplicationValidationService, ListingApplicationValidationService>();
builder.Services.AddScoped<ITicketValidationService, TicketValidationService>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueueHostedService>();

builder.Services.AddScoped<IHeaderService<VenueHeaderDto>, VenueService>();
builder.Services.AddScoped<IHeaderService<ArtistHeaderDto>, ArtistService>();
builder.Services.AddScoped<IHeaderService<EventHeaderDto>, EventService>();

builder.Services.AddSingleton<IHeaderServiceFactory, HeaderServiceFactory>();
builder.Services.AddSingleton<IReviewServiceMethodFactory, ReviewServiceMethodFactory>();

//Lazy Dependency Injections
builder.Services.AddScoped(provider => new Lazy<IEventService>(() => provider.GetRequiredService<IEventService>()));
builder.Services.AddScoped(provider => new Lazy<ITicketService>(() => provider.GetRequiredService<ITicketService>()));

builder.Services.AddHttpClient<IGeocodingService, GeocodingService>(client =>
{
    client.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/geocode/");
});

// Repositories
builder.Services.AddScoped<IVenueRepository, VenueRepository>();
builder.Services.AddScoped<IArtistRepository, ArtistRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IListingApplicationRepository, ListingApplicationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IListingRepository, ListingRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IPreferenceRepository, PreferenceRepository>();
builder.Services.AddScoped<IStripeEventRepository, StripeEventRepository>();

builder.Services.AddScoped<IHeaderRepository<Venue, VenueHeaderDto>, VenueRepository>();
builder.Services.AddScoped<IHeaderRepository<Artist, ArtistHeaderDto>, ArtistRepository>();
builder.Services.AddScoped<IHeaderRepository<Event, EventHeaderDto>, EventRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddSignalR();

builder.Services.AddAuthentication();
builder.Services.ConfigureApplicationCookie(config =>
{
    config.Cookie.Name = "Identity.Cookie";
    config.Cookie.SameSite = SameSiteMode.None;
    config.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    config.ExpireTimeSpan = TimeSpan.FromDays(7);
    //Prevent automatic url redirect on http errors and replaces with the actual error
    config.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };

    config.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
});
builder.Services.AddAuthorization()
    .AddSingleton<IAuthorizationHandler, AdminAuthorizeHandler>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGroup("/api").MapIdentityApi<ApplicationUser>();
app.MapHub<PaymentHub>("/hub/payments");
app.MapHub<EventHub>("/hub/events");

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallback(async context =>
{
    // If its an invalid API request
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        // Return a 404 (Useful for debugging purposes and user clarity when returning api data)
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    // If it's not an API request, continue to index.html
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.UseExceptionHandler();

//Development use
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Initialize the Database
try
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    await ApplicationDbInitializer.InitializeAsync(context, userManager);

}
catch(Exception)
{
    Console.WriteLine("oops");
}

app.Run();
