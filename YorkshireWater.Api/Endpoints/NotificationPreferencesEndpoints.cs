using Microsoft.AspNetCore.Http.HttpResults;
using YorkshireWater.Api.Contracts;
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

        private static ProblemHttpResult CustomerNotFound(Guid customerId) =>
            TypedResults.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Customer not found",
                detail: $"No customer with id '{customerId}' exists.");
    }
}
