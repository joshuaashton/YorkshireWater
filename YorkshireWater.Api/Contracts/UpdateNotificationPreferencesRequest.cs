using System.ComponentModel.DataAnnotations;
using YorkshireWater.Api.Domain;

namespace YorkshireWater.Api.Contracts
{
    /// <summary>
    /// Both flags are nullable so that a missing property is detected by validation instead of silently defaulting to false (FR3).
    /// </summary>
    public sealed class UpdateNotificationPreferencesRequest
    {
        [Required]
        public bool? EmailEnabled { get; init; }

        [Required]
        public bool? SmsEnabled { get; init; }

        /// <summary>Only call after validation has passed; both values are then guaranteed non-null.</summary>
        public NotificationPreferences ToDomain() => new(EmailEnabled!.Value, SmsEnabled!.Value);
    }
}
