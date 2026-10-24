using Microsoft.AspNetCore.Http.HttpResults;
using YorkshireWater.Api.Contracts;
using YorkshireWater.Api.Domain;
using YorkshireWater.Api.Services;

namespace YorkshireWater.Api.Endpoints
{
    public static class NotificationPreferencesEndpoints
    {
        // No ":guid" route constraint on purpose: a constraint would produce 404 for a malformed id,
        // whereas a binding failure on the Guid parameter gives the 400 the specification asks for
        private const string Route = "/api/customers/{customerId}/notification-preferences";

        public static IEndpointRouteBuilder MapNotificationPreferencesEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup(Route).WithTags("Notification preferences");

            group.MapGet("", GetPreferences).WithName("GetNotificationPreferences");
            group.MapPut("", UpdatePreferences).WithName("UpdateNotificationPreferences");

            return app;
        }

        private static async Task<Results<Ok<NotificationPreferencesResponse>, ProblemHttpResult>> GetPreferences(
            Guid customerId,
            INotificationPreferencesService service,
            CancellationToken cancellationToken)
        {
            var preferences = await service.GetAsync(customerId, cancellationToken);

            return preferences is null
                ? CustomerNotFound(customerId)
                : TypedResults.Ok(NotificationPreferencesResponse.From(customerId, preferences));
        }

        private static async Task<Results<Ok<NotificationPreferencesResponse>, ProblemHttpResult>> UpdatePreferences(
            Guid customerId,
            UpdateNotificationPreferencesRequest request,
            INotificationPreferencesService service,
            CancellationToken cancellationToken)
        {
            var result = await service.UpdateAsync(customerId, request.ToDomain(), cancellationToken);

            return result switch
            {
                UpdatePreferencesResult.Updated updated =>
                    TypedResults.Ok(NotificationPreferencesResponse.From(customerId, updated.Preferences)),

                UpdatePreferencesResult.CustomerNotFound =>
                    CustomerNotFound(customerId),

                UpdatePreferencesResult.NotificationFailed =>
                    TypedResults.Problem(
                        statusCode: StatusCodes.Status503ServiceUnavailable,
                        title: "Preferences saved but notification failed",
                        detail: "The preferences were stored, but the notification system could not be informed of the change."),

                _ => throw new InvalidOperationException($"Unhandled result type {result.GetType().Name}"),
            };
        }

        private static ProblemHttpResult CustomerNotFound(Guid customerId) =>
            TypedResults.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Customer not found",
                detail: $"No customer with id '{customerId}' exists.");
    }
}
