using YorkshireWater.Api.Domain;

namespace YorkshireWater.Api.Services
{
    public sealed class NotificationPreferencesService(
        INotificationPreferencesRepository repository, 
        IPreferenceChangeNotifier notifier, 
        ILogger<NotificationPreferencesService> logger) : INotificationPreferencesService
    {
        public Task<NotificationPreferences?> GetAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return repository.GetAsync(customerId, cancellationToken);
        }

        public async Task<UpdatePreferencesResult> UpdateAsync(Guid customerId, NotificationPreferences requested, CancellationToken cancellationToken = default)
        {
            var current = await repository.GetAsync(customerId, cancellationToken);

            if (current is null)
                return new UpdatePreferencesResult.CustomerNotFound();

            if (current == requested)
            {
                // Nothing changed so nothing to notify about
                return new UpdatePreferencesResult.Updated(current);
            }

            await repository.UpdateAsync(customerId, requested, cancellationToken);

            try
            {
                await notifier.NotifyAsync(customerId, requested, cancellationToken);
            }
            // Only a cancellation the caller asked for propagates. A dependency timeout also throws an
            // OperationCanceledException (TaskCanceledException) and is a notification failure like any other
            catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Preferences for customer {CustomerId} were saved but the change notification failed", customerId);

                return new UpdatePreferencesResult.NotificationFailed(requested);
            }

            return new UpdatePreferencesResult.Updated(requested);
        }
    }
}
