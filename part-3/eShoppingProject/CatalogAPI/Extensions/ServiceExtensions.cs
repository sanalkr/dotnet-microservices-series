using CatalogAPI.IntegrationEvents;
using EventBus.Contracts;
using EventBus;
using Microsoft.Extensions.Options;
using EventServiceBus;
using CatalogAPI.Settings;

namespace CatalogAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddIntegrationEventServices(this IServiceCollection services)
        {

            services.AddSingleton<IServiceBusConnection>(sp =>
            {
                var setting = sp.GetRequiredService<IOptions<CatalogSettings>>().Value;

                return new DefaultServiceBusConnection(setting.EventBusConnectionString);
            });

            return services;
        }

        public static IServiceCollection AddEventBus(this IServiceCollection services)
        {
            services.AddSingleton<IEventBusSubscriptionManager, InMemoryEventBusSubscriptionsManager>();

            services.AddSingleton<IEventBus, ServiceBusEventBus>(sp =>
            {
                var serviceBusConnection = sp.GetRequiredService<IServiceBusConnection>();
                var logger = sp.GetRequiredService<ILogger<ServiceBusEventBus>>();
                var subscriptionManager = sp.GetRequiredService<IEventBusSubscriptionManager>();
                var setting = sp.GetRequiredService<IOptions<CatalogSettings>>().Value;

                return new ServiceBusEventBus(serviceBusConnection, logger,
                    subscriptionManager, sp, setting.SubscriptionClientName);
            });

            services.AddScoped<CatalogIntegrationEventService>();

            return services;
        }
    }
}
