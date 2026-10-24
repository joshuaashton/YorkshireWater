using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
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

        private Task<HttpResponseMessage> PutRawAsync(Guid customerId, string body) => 
            _client.PutAsync(Url(customerId), new StringContent(body, Encoding.UTF8, "application/json"));

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

        // AC08: a well formed body with a missing or null property fails validation whilst naming the property.
        [Theory]
        [InlineData("""{ "emailEnabled": true }""", "SmsEnabled")]
        [InlineData("""{ "smsEnabled": true }""", "EmailEnabled")]
        [InlineData("""{ "emailEnabled": null, "smsEnabled": true }""", "EmailEnabled")]
        [InlineData("""{}""", "EmailEnabled")]
        public async Task Put_Returns400NamingTheProperty_WhenPropertyMissing(string body, string expectedErrorKey)
        {
            var response = await PutRawAsync(SeedData.Josh, body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var problem = await response.Content.ReadFromJsonAsync<JsonObject>();

            Assert.True(problem!["errors"]!.AsObject().ContainsKey(expectedErrorKey));
        }

        // AC08: a body that cannot be read at all is rejected before validation with an explanation
        [Theory]
        [InlineData("""{ "emailEnabled": "yes", "smsEnabled": true }""")]
        [InlineData("")]
        [InlineData("not json")]
        public async Task Put_Returns400WithDetail_WhenBodyIsMalformed(string body)
        {
            var response = await PutRawAsync(SeedData.Josh, body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var problem = await response.Content.ReadFromJsonAsync<JsonObject>();

            Assert.False(string.IsNullOrWhiteSpace((string?)problem!["detail"]));
        }

        // AC08: whichever way the body is invalid, nothing is stored and nothing is notified
        [Theory]
        [InlineData("""{ "emailEnabled": true }""")]
        [InlineData("not json")]
        public async Task Put_DoesNotMutateOrNotify_ForInvalidBody(string body)
        {
            var before = await _client.GetFromJsonAsync<NotificationPreferencesResponse>(Url(SeedData.Josh));

            await PutRawAsync(SeedData.Josh, body);

            Assert.Equal(before, await _client.GetFromJsonAsync<NotificationPreferencesResponse>(Url(SeedData.Josh)));
            AssertNoNotificationWasRequested();
        }

        // AC09
        [Fact]
        public async Task Put_Returns400_ForInvalidGuid()
        {
            var response = await _client.PutAsJsonAsync(Url("not-a-guid"), new { emailEnabled = true, smsEnabled = true });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // AC10
        [Fact]
        public async Task Put_Returns503AndKeepsChanges_WhenNotifierFails()
        {
            _factory.Notifier
                .Setup(n => n.NotifyAsync(It.IsAny<Guid>(), It.IsAny<NotificationPreferences>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("notification system down"));

            var response = await _client.PutAsJsonAsync(Url(SeedData.Josh), new { emailEnabled = false, smsEnabled = true });

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

            var problem = await response.Content.ReadFromJsonAsync<JsonObject>();

            Assert.Equal(503, (int)problem!["status"]!);

            var stored = await _client.GetFromJsonAsync<NotificationPreferencesResponse>(Url(SeedData.Josh));

            Assert.Equal(new NotificationPreferencesResponse(SeedData.Josh, EmailEnabled: false, SmsEnabled: true), stored);
        }

        [Fact]
        public async Task Put_Returns503AndKeepsChanges_WhenNotifierTimesOut()
        {
            _factory.Notifier
                .Setup(n => n.NotifyAsync(It.IsAny<Guid>(), It.IsAny<NotificationPreferences>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TaskCanceledException("notification system timed out"));

            var response = await _client.PutAsJsonAsync(Url(SeedData.Josh), new { emailEnabled = false, smsEnabled = true });

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

            var stored = await _client.GetFromJsonAsync<NotificationPreferencesResponse>(Url(SeedData.Josh));

            Assert.Equal(new NotificationPreferencesResponse(SeedData.Josh, EmailEnabled: false, SmsEnabled: true), stored);
        }
    }
}
