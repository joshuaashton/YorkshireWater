namespace YorkshireWater.Api.Domain
{
    /// <summary>Represents the external notification system that must be told when preferences change</summary>
    public interface IPreferenceChangeNotifier
    {
        Task NotifyAsync(Guid customerId, NotificationPreferences preferences, CancellationToken cancellationToken = default);
    }
}
