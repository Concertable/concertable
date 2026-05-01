using Concertable.Application.Interfaces;
using Concertable.Application.Serializers;
using Concertable.Web;
using Concertable.Artist.Api.Extensions;
using Concertable.Artist.Infrastructure.Extensions;
using Concertable.Venue.Api.Extensions;
using Concertable.Venue.Infrastructure.Extensions;
using Concertable.Concert.Api.Extensions;
using Concertable.Concert.Infrastructure.Extensions;
using Concertable.Contract.Api.Extensions;
using Concertable.Contract.Infrastructure.Extensions;
using Concertable.Payment.Api.Extensions;
using Concertable.Payment.Infrastructure.Extensions;
using Concertable.Messaging.Infrastructure.Extensions;
using Concertable.Customer.Api.Extensions;
using Concertable.Customer.Infrastructure.Extensions;
using Concertable.Authorization.Infrastructure.Extensions;
using Concertable.User.Api.Extensions;
using Concertable.User.Infrastructure.Extensions;
using Concertable.Data.Infrastructure.Extensions;
using Concertable.Shared.Infrastructure.Extensions;
using Concertable.Seeding.Fakers;
using Concertable.Search.Api.Extensions;
using Concertable.Web.Extensions;
using Concertable.Notification.Infrastructure.Hubs;
using Concertable.Notification.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.IncludeFields = true;
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddLogging();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName);
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithOrigins("http://localhost:4200", "https://localhost:4200", "http://localhost:5173", "https://localhost:5173");
    });
});

var services = builder.Services;

services.AddScoped<IKeyedServiceProvider>(sp => (IKeyedServiceProvider)sp);

services.AddInfrastructure(builder.Configuration);
if (!builder.Environment.IsEnvironment("Testing"))
{
    services.AddScoped<IDbInitializer, DevDbInitializer>();
    services.AddScoped<Concertable.Seeding.SeedData>();
    services.AddScoped<ILocationFaker, LocationFaker>();
    services.AddSharedDevSeeder();
    services.AddUserDevSeeder();
    services.AddArtistDevSeeder();
    services.AddVenueDevSeeder();
    services.AddContractDevSeeder();
    services.AddConcertDevSeeder();
    services.AddPaymentDevSeeder();
    services.AddMessagingDevSeeder();
    services.AddCustomerDevSeeder();
}
services.AddServices(builder.Configuration);
services.AddRepositories();
services.AddNotificationModule();
services.AddMessagingApi(builder.Configuration);
services.AddSearchApi(builder.Configuration);
services.AddArtistApi(builder.Configuration);
services.AddVenueApi(builder.Configuration);
services.AddConcertApi(builder.Configuration);
services.AddContractApi(builder.Configuration);
services.AddPaymentApi(builder.Configuration);
services.AddCustomerApi(builder.Configuration);
services.AddQueueHostedService();
services.AddAuthorizationModule();
services.AddUserApi(builder.Configuration);
services.AddAuth(builder.Configuration);
services.AddValidation();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapDefaultEndpoints();
app.MapControllers();
app.MapHub<NotificationHub>("/hub/notifications");
app.MapGet("/health", () => Results.Ok());

if (app.Environment.IsEnvironment("E2E"))
    app.MapE2EEndpoints();

app.MapFallback(async context =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    await context.Response.SendFileAsync("wwwroot/index.html");
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.InitializeAsync();
}

app.Run();

public partial class Program { }
