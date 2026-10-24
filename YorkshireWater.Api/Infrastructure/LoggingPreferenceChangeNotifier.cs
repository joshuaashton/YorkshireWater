using YorkshireWater.Api.Domain;

namespace YorkshireWater.Api.Infrastructure
{
    /// <summary>Stand-in for the real notification system</summary>
    public sealed class LoggingPreferenceChangeNotifier(ILogger<LoggingPreferenceChangeNotifier> logger) : IPreferenceChangeNotifier
    {
        public Task NotifyAsync(Guid customerId, NotificationPreferences preferences, CancellationToken cancellationToken = default)
        {
            logger.LogInformation(
                "Notification requested for customer {CustomerId}: email={EmailEnabled}, sms={SmsEnabled}",
                customerId, preferences.EmailEnabled, preferences.SmsEnabled);

            return Task.CompletedTask;
        }
    }
}
