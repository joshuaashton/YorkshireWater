using Moq;
using YorkshireWater.Api.Domain;
using YorkshireWater.Api.Services;

namespace YorkshireWater.Tests.Unit
{
    public class NotificationPreferencesServiceTests
    {
        private static readonly Guid CustomerId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        private static readonly NotificationPreferences Current = new(EmailEnabled: true, SmsEnabled: false);
        private static readonly NotificationPreferences Changed = new(EmailEnabled: false, SmsEnabled: true);

        private readonly Mock<INotificationPreferencesRepository> _repository = new();
        private readonly Mock<IPreferenceChangeNotifier> _notifier = new();
        private readonly NotificationPreferencesService _service;

        public NotificationPreferencesServiceTests()
        {
            _service = new NotificationPreferencesService(_repository.Object, _notifier.Object);
        }

        private void AssertNothingWasPersisted()
        {
            _repository.Verify(
                r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<NotificationPreferences>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }


        private void AssertNoNotificationWasRequested()
        {
            _notifier.Verify(
                n => n.NotifyAsync(It.IsAny<Guid>(), It.IsAny<NotificationPreferences>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetAsync_ReturnsRepositoryValue()
        {
            _repository.Setup(r => r.GetAsync(CustomerId, It.IsAny<CancellationToken>())).ReturnsAsync(Current);

            var preferences = await _service.GetAsync(CustomerId);

            Assert.Equal(Current, preferences);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsCustomerNotFound_AndDoesNotNotify_WhenCustomerUnknown()
        {
            _repository.Setup(r => r.GetAsync(CustomerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((NotificationPreferences?)null);

            var result = await _service.UpdateAsync(CustomerId, Changed);

            Assert.IsType<UpdatePreferencesResult.CustomerNotFound>(result);
            AssertNothingWasPersisted();
            AssertNoNotificationWasRequested();
        }

        [Fact]
        public async Task UpdateAsync_PersistsAndNotifiesOnce_WhenPreferencesChange()
        {
            _repository.Setup(r => r.GetAsync(CustomerId, It.IsAny<CancellationToken>())).ReturnsAsync(Current);

            var result = await _service.UpdateAsync(CustomerId, Changed);

            var updated = Assert.IsType<UpdatePreferencesResult.Updated>(result);

            Assert.Equal(Changed, updated.Preferences);

            _repository.Verify(r => r.UpdateAsync(CustomerId, Changed, It.IsAny<CancellationToken>()), Times.Once);
            _notifier.Verify(n => n.NotifyAsync(CustomerId, Changed, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}