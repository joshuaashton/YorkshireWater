namespace YorkshireWater.Api.Domain
{
    public interface INotificationPreferencesRepository
    {
        /// <summary>Returns the customer's preferences or null if the customer does not exist.</summary>
        Task<NotificationPreferences?> GetAsync(Guid customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces the preferences of an existing customer or throws an exception if the customer does not exist
        /// </summary>
        /// <exception cref="InvalidOperationException">The customer does not exist.</exception>
        Task UpdateAsync(Guid customerId, NotificationPreferences preferences, CancellationToken cancellationToken = default);
    }
}
