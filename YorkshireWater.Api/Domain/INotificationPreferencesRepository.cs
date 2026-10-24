namespace YorkshireWater.Api.Domain
{
    public interface INotificationPreferencesRepository
    {
        /// <summary>Returns the customer's preferences or null if the customer does not exist.</summary>
        Task<NotificationPreferences?> GetAsync(Guid customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces the preferences of an existing customer. Returns false if the customer does not exist
        /// </summary>
        Task<bool> UpdateAsync(Guid customerId, NotificationPreferences preferences, CancellationToken cancellationToken = default);
    }
}
