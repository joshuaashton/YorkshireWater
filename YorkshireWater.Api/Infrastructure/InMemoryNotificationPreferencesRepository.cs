using System.Collections.Concurrent;
using YorkshireWater.Api.Domain;

namespace YorkshireWater.Api.Infrastructure
{
    /// <summary>A thread safe in memory store</summary>
    public sealed class InMemoryNotificationPreferencesRepository(IReadOnlyDictionary<Guid, NotificationPreferences> initialData) : INotificationPreferencesRepository
    {
        private readonly ConcurrentDictionary<Guid, NotificationPreferences> _store = new(initialData);

        public Task<NotificationPreferences?> GetAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            _store.TryGetValue(customerId, out var preferences);

            return Task.FromResult(preferences);
        }

        public Task UpdateAsync(Guid customerId, NotificationPreferences preferences, CancellationToken cancellationToken = default)
        {
            if (!_store.ContainsKey(customerId))
                throw new InvalidOperationException($"Customer '{customerId}' does not exist");

            _store[customerId] = preferences;
            return Task.CompletedTask;
        }
    }
}
