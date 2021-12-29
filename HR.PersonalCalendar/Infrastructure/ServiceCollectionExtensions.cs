using HR.WebUntisConnector;
using HR.WebUntisConnector.Configuration;
using HR.WebUntisConnector.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace HR.PersonalCalendar.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds both the <see cref="IApiClientFactory"/> and <see cref="ICachedApiClientFactory"/> services to the service collection.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="serviceLifetime"></param>
        /// <returns></returns>
        public static IServiceCollection AddCachedApiClientFactory(this IServiceCollection services, WebUntisConfigurationSection configuration, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            services.AddApiClientFactory(configuration, serviceLifetime);

            services.Add(new ServiceDescriptor(
                serviceType: typeof(ICachedApiClientFactory),
                provider => new CachedApiClientFactory(provider.GetRequiredService<IApiClientFactory>()),
                serviceLifetime));

            return services;
        }
    }
}
