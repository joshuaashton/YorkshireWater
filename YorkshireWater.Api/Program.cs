using YorkshireWater.Api.Domain;
using YorkshireWater.Api.Endpoints;
using YorkshireWater.Api.Infrastructure;
using Scalar.AspNetCore;
using YorkshireWater.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Minimal API validation (.NET 10..) DataAnnotations on endpoint parameters produce 400 ValidationProblemDetails.
builder.Services.AddValidation();

// Binding failures (malformed JSON, empty body, non-GUID id etc etc) would otherwise be written as an
// empty 400. Throwing instead lets the exception handler below turn them into problem details.
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context =>
    {
        if (context.Exception is BadHttpRequestException badRequest)
            context.ProblemDetails.Detail = badRequest.Message;
    });

builder.Services.AddSingleton<INotificationPreferencesRepository>(_ => new InMemoryNotificationPreferencesRepository(SeedData.Customers));
builder.Services.AddSingleton<IPreferenceChangeNotifier, LoggingPreferenceChangeNotifier>();
builder.Services.AddScoped<INotificationPreferencesService, NotificationPreferencesService>();

var app = builder.Build();

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    // A BadHttpRequestException is the client's fault: keep its status (400) and do not log it as a failure
    StatusCodeSelector = exception => exception is BadHttpRequestException badRequest ? badRequest.StatusCode : StatusCodes.Status500InternalServerError,
    SuppressDiagnosticsCallback = context => context.Exception is BadHttpRequestException,
});

// OpenAPI document at /openapi/v1.json and the Scalar UI at /scalar, development only for easy reviewer testing
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapNotificationPreferencesEndpoints();

app.Run();

// Exposes the implicit Program class to the integration tests (WebApplicationFactory<Program>).
public partial class Program;