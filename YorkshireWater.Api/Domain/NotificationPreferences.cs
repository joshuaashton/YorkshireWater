namespace YorkshireWater.Api.Domain
{
    /// <summary>
    /// A customer's notification preferences (a record so that value equality comes free for comparisons).    
    /// </summary>
    public sealed record NotificationPreferences(bool EmailEnabled, bool SmsEnabled);
}
