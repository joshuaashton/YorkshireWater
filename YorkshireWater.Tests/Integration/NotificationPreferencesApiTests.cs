using System.Net;
using System.Net.Http.Json;
using YorkshireWater.Api.Contracts;
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
    }
}
