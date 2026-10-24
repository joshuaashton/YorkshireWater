using YorkshireWater.Api.Domain;

namespace YorkshireWater.Api.Services
{
    public interface INotificationPreferencesService
    {
        Task<NotificationPreferences?> GetAsync(Guid customerId, CancellationToken cancellationToken = default);
        Task<UpdatePreferencesResult> UpdateAsync(Guid customerId, NotificationPreferences requested, CancellationToken cancellationToken = default);
    }
}
