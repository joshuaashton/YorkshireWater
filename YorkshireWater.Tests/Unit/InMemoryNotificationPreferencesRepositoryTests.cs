using YorkshireWater.Api.Domain;
using YorkshireWater.Api.Infrastructure;

namespace YorkshireWater.Tests.Unit
{
    public class InMemoryNotificationPreferencesRepositoryTests
    {
        private static readonly Guid KnownCustomer = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        private static readonly Guid UnknownCustomer = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        private readonly InMemoryNotificationPreferencesRepository _repository;

        public InMemoryNotificationPreferencesRepositoryTests()
        {
            var seedData = new Dictionary<Guid, NotificationPreferences>
            {
                { KnownCustomer, new NotificationPreferences(EmailEnabled: true, SmsEnabled: false) }
            };

            _repository = new InMemoryNotificationPreferencesRepository(seedData);
        }

        [Fact]
        public async Task GetAsync_ReturnsPreferences_ForKnownCustomer()
        {
            var preferences = await _repository.GetAsync(KnownCustomer);

            Assert.Equal(new NotificationPreferences(EmailEnabled: true, SmsEnabled: false), preferences);
        }

        [Fact]
        public async Task GetAsync_ReturnsNull_ForUnknownCustomer()
        {
            var preferences = await _repository.GetAsync(UnknownCustomer);

            Assert.Null(preferences);
        }

        [Fact]
        public async Task UpdateAsync_ReplacesPreferences_ForKnownCustomer()
        {
            var updated = new NotificationPreferences(EmailEnabled: false, SmsEnabled: true);

            await _repository.UpdateAsync(KnownCustomer, updated);

            Assert.Equal(updated, await _repository.GetAsync(KnownCustomer));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsAndDoesNotCreateCustomer_ForUnknownCustomer()
        {
            var preferences = new NotificationPreferences(EmailEnabled: true, SmsEnabled: true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.UpdateAsync(UnknownCustomer, preferences));

            Assert.Null(await _repository.GetAsync(UnknownCustomer));
        }
    }
}
