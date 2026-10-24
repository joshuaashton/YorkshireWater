using Moq;
using System.Net;
using System.Net.Http.Json;
using YorkshireWater.Api.Contracts;
using YorkshireWater.Api.Domain;
using YorkshireWater.Api.Infrastructure;

namespace YorkshireWater.Tests.Integration
{
    public class NotificationPreferencesApiTests : IDisposable
    {
        private static readonly Guid UnknownCustomer = Guid.Parse("00000000-0000-0000-0000-000000000001");

        private readonly ApiTestFactory _factory = new ApiTestFactory();
        private readonly HttpClient _client;

        public NotificationPreferencesApiTests()
        {
            _client = _factory.CreateClient();
        }

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        private static string Url(Guid customerId) => Url(customerId.ToString());
        private static string Url(string customerId) => $"/api/customers/{customerId}/notification-preferences";

        private void AssertNoNotificationWasRequested() =>
            _factory.Notifier.Verify(
            n => n.NotifyAsync(It.IsAny<Guid>(), It.IsAny<NotificationPreferences>(), It.IsAny<CancellationToken>()),
            Times.Never);

        // AC01
        [Fact]
        public async Task Get_ReturnsPreferences_ForSeededCustomer()
        {
            var response = await _client.GetAsync(Url(SeedData.Josh));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<NotificationPreferencesResponse>();

            Assert.Equal(new NotificationPreferencesResponse(SeedData.Josh, EmailEnabled: true, SmsEnabled: false), body);
        }

        // AC02
        [Fact]
        public async Task Get_Returns404_ForUnknownCustomer()
        {
            var response = await _client.GetAsync(Url(UnknownCustomer));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // AC09
        [Fact]
        public async Task Get_Returns400_ForInvalidGuid()
        {
            var response = await _client.GetAsync(Url("not-a-guid"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // AC03 + AC04
        [Fact]
        public async Task Put_PersistsAndReturnsUpdatedPreferences_ForSeededCustomer()
        {
            var response = await _client.PutAsJsonAsync(Url(SeedData.Josh), new { emailEnabled = false, smsEnabled = true });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var expected = new NotificationPreferencesResponse(SeedData.Josh, EmailEnabled: false, SmsEnabled: true);

            Assert.Equal(expected, await response.Content.ReadFromJsonAsync<NotificationPreferencesResponse>());
            Assert.Equal(expected, await _client.GetFromJsonAsync<NotificationPreferencesResponse>(Url(SeedData.Josh)));
        }

        // AC06
        [Fact]
        public async Task Put_RequestsNotificationOnce_WhenPreferencesChange()
        {
            await _client.PutAsJsonAsync(Url(SeedData.Josh), new { emailEnabled = false, smsEnabled = true });

            _factory.Notifier.Verify(
                n => n.NotifyAsync(
                    SeedData.Josh,
                    new NotificationPreferences(EmailEnabled: false, SmsEnabled: true),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // AC05
        [Fact]
        public async Task Put_DoesNotRequestNotification_WhenPreferencesUnchanged()
        {
            var response = await _client.PutAsJsonAsync(Url(SeedData.Josh), new { emailEnabled = true, smsEnabled = false });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            AssertNoNotificationWasRequested();
        }

        // AC07
        [Fact]
        public async Task Put_Returns404AndDoesNotNotify_ForUnknownCustomer()
        {
            var response = await _client.PutAsJsonAsync(Url(UnknownCustomer), new { emailEnabled = true, smsEnabled = true });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            AssertNoNotificationWasRequested();
        }
    }
}
