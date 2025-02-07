using Application.Mappings;
using Core.Entities.Identity;
using Core.Interfaces;
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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.IncludeFields = true; //Include nested fields 

    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles; //Ignore cycles
    options.JsonSerializerOptions.WriteIndented = true; //JSON formatting
});
builder.Services.AddHttpContextAccessor();

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


builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<IHeaderService, HeaderService>();

// Repositories
builder.Services.AddScoped<IVenueRepository, VenueRepository>();
builder.Services.AddScoped<IArtistRepository, ArtistRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IRegisterRepository, RegisterRepository>();
builder.Services.AddScoped<IListingRepository, ListingRepository>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

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
builder.Services.AddAuthorization();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGroup("/api").MapIdentityApi<ApplicationUser>();

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

System.Diagnostics.Debug.WriteLine("hello");
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
