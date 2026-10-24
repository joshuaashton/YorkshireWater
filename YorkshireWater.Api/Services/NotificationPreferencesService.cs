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
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Preferences for customer {CustomerId} were saved but the change notification failed", customerId);

                return new UpdatePreferencesResult.NotificationFailed(requested);
            }

            return new UpdatePreferencesResult.Updated(requested);
        }
    }
}
