using YorkshireWater.Api.Domain;

namespace YorkshireWater.Api.Contracts
{
    public sealed record NotificationPreferencesResponse(Guid CustomerId, bool EmailEnabled, bool SmsEnabled)
    {
        public static NotificationPreferencesResponse From(Guid customerId, NotificationPreferences preferences) => new(customerId, preferences.EmailEnabled, preferences.SmsEnabled);
    }
}
