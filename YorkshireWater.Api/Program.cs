using YorkshireWater.Api.Domain;
using YorkshireWater.Api.Endpoints;
using YorkshireWater.Api.Infrastructure;
using YorkshireWater.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

builder.Services.AddSingleton<INotificationPreferencesRepository>(_ => new InMemoryNotificationPreferencesRepository(SeedData.Customers));
builder.Services.AddSingleton<IPreferenceChangeNotifier, LoggingPreferenceChangeNotifier>();
builder.Services.AddScoped<INotificationPreferencesService, NotificationPreferencesService>();

var app = builder.Build();

app.UseExceptionHandler();

app.MapNotificationPreferencesEndpoints();

app.Run();

// Exposes the implicit Program class to the integration tests (WebApplicationFactory<Program>).
public partial class Program;