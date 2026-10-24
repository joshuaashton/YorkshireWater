using YorkshireWater.Api.Domain;

namespace YorkshireWater.Api.Infrastructure
{
    /// <summary>Fixed, documented customer IDs so the API can be demonstrated without setup (FR6).</summary>
    public static class SeedData
    {
        public static readonly Guid Josh = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid Steve = Guid.Parse("22222222-2222-2222-2222-222222222222");

        public static IReadOnlyDictionary<Guid, NotificationPreferences> Customers { get; } =
            new Dictionary<Guid, NotificationPreferences>
            {
                { Josh, new NotificationPreferences(EmailEnabled: true, SmsEnabled: false) },
                { Steve, new NotificationPreferences(EmailEnabled: false, SmsEnabled: true) }
            };
    }
}
