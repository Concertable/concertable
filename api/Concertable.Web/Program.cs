using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Auth;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Serializers;
using Concertable.Core.ModelBinders;
using Concertable.Infrastructure.Data;
using Concertable.Seeding;
using Concertable.Seeding.Fakers;
using Concertable.Web.Extensions;
using Concertable.Web.Hubs;
using Concertable.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers(opts =>
    opts.ModelBinderProviders.Insert(0, new CommaDelimitedIntArrayBinderProvider()))
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

services.AddInfrastructure(builder.Configuration);
if (!builder.Environment.IsEnvironment("Testing"))
{
    services.AddScoped<IDbInitializer, DevDbInitializer>();
    services.AddScoped<Concertable.Seeding.SeedData>();
    services.AddScoped<ILocationFaker, LocationFaker>();
}
services.AddServices(builder.Configuration);
services.AddRepositories();
services.AddSearch();
services.AddAuth(builder.Configuration);
services.AddValidation();

builder.Services.AddExceptionHandler<Infrastructure.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CurrentUserMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapDefaultEndpoints();
app.MapControllers();
app.MapHub<NotificationHub>("/hub/notifications");
app.MapGet("/health", () => Results.Ok());

if (app.Environment.IsEnvironment("E2E"))
{
    app.MapPost("/e2e/reseed", async (IDbInitializer dbInitializer, SeedData seedData) =>
    {
        await dbInitializer.InitializeAsync();
        return Results.Ok(new SeedDataResponse
        {
            TestPassword = SeedData.TestPassword,
            Customer = new SeededUser { Email = seedData.Customer.Email },
            VenueManager1 = new SeededVenueManager { StripeAccountId = seedData.VenueManager1.StripeAccountId },
            ArtistManager = new SeededArtistManager { StripeAccountId = seedData.ArtistManager.StripeAccountId },
            PostedFlatFeeApp = new SeededApplication { ApplicationId = seedData.PostedFlatFeeApp.Id, Concert = new SeededConcert { Id = seedData.PostedFlatFeeApp.Concert!.Id } },
            PostedDoorSplitApp = new SeededApplication { ApplicationId = seedData.PostedDoorSplitApp.Id, Concert = new SeededConcert { Id = seedData.PostedDoorSplitApp.Concert!.Id } },
            PostedVersusApp = new SeededApplication { ApplicationId = seedData.PostedVersusApp.Id, Concert = new SeededConcert { Id = seedData.PostedVersusApp.Concert!.Id } },
            PostedVenueHireApp = new SeededApplication { ApplicationId = seedData.PostedVenueHireApp.Id, Concert = new SeededConcert { Id = seedData.PostedVenueHireApp.Concert!.Id } },
            FinishedDoorSplitApp = new SeededApplication { ApplicationId = seedData.FinishedDoorSplitApp.Id, Concert = new SeededConcert { Id = seedData.FinishedDoorSplitApp.Concert!.Id } },
            FinishedVersusApp = new SeededApplication { ApplicationId = seedData.FinishedVersusApp.Id, Concert = new SeededConcert { Id = seedData.FinishedVersusApp.Concert!.Id } },
        });
    });

    app.MapPost("/e2e/finish/{concertId:int}", async (int concertId, IFinishedProcessor finishedProcessor) =>
    {
        var result = await finishedProcessor.FinishedAsync(concertId);
        return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Errors.Select(e => e.Message));
    });
}

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
