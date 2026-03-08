using Core.Entities.Identity;
using Infrastructure.Data;
using Infrastructure.Data.Identity;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
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
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithOrigins("http://localhost:4200", "https://localhost:4200");
    });
});

var services = builder.Services;

services.AddInfrastructure(builder.Configuration);
services.AddServices();
services.AddRepositories();
services.AddSearch();
services.AddAuth();
services.AddValidation();

builder.Services.AddExceptionHandler<Infrastructure.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CurrentUserMiddleware>();

app.MapControllers();
app.MapGroup("/api").MapIdentityApi<ApplicationUser>();
app.MapHub<PaymentHub>("/hub/payments");
app.MapHub<ConcertHub>("/hub/concerts");

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallback(async context =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

try
{
    using var scope = app.Services.CreateScope();
    var serviceProvider = scope.ServiceProvider;
    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await ApplicationDbInitializer.InitializeAsync(context, userManager);
}
catch (Exception)
{
    Console.WriteLine("oops");
}

app.Run();
