using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using YorkshireWater.Api.Domain;


namespace YorkshireWater.Tests.Integration
{
    /// <summary>
    /// Boots the real application in memory with the notifier swapped for a mock
    /// </summary>
    public class ApiTestFactory : WebApplicationFactory<Program>
    {
        public Mock<IPreferenceChangeNotifier> Notifier { get; } = new Mock<IPreferenceChangeNotifier>();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services => services.Replace(ServiceDescriptor.Singleton(Notifier.Object)));

        }
    }
}