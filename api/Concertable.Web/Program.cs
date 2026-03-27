using Application.Interfaces;
using Application.Interfaces.Auth;
using Infrastructure.Data;
using Infrastructure.Data.Identity;
using Web.Extensions;
using Web.Hubs;
using Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
Console.WriteLine("Stripe__SecretKey: " + Environment.GetEnvironmentVariable("Stripe__SecretKey")?.Substring(0, 8) ?? "NULL");

builder.Services.AddControllers(opts =>
    opts.ModelBinderProviders.Insert(0, new Core.ModelBinders.CommaDelimitedIntArrayBinderProvider()))
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.IncludeFields = true;
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.Converters.Add(new Application.Serializers.TimeOnlyJsonConverter());
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
    services.AddScoped<IDbInitializer, ApplicationDbInitializer>();
services.AddServices();
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

app.MapControllers();
app.MapHub<NotificationHub>("/hub/concerts");
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
