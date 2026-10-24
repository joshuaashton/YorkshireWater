using YorkshireWater.Api.Domain;

namespace YorkshireWater.Api.Services
{
    public sealed class NotificationPreferencesService(INotificationPreferencesRepository repository, IPreferenceChangeNotifier notifier) : INotificationPreferencesService
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

            await repository.UpdateAsync(customerId, requested, cancellationToken);
            await notifier.NotifyAsync(customerId, requested, cancellationToken);

            return new UpdatePreferencesResult.Updated(requested);
        }
    }
}
