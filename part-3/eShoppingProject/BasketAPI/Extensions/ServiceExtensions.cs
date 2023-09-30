using BasketAPI.IntegrationEvents.Events;
using BasketAPI.IntegrationEvents.Handlers;
using BasketAPI.Settings;
using EventBus;
using EventBus.Contracts;
using EventServiceBus;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace BasketAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddRedis(this IServiceCollection services)
        {
            services.AddSingleton(sp =>
            {
                var setting = sp.GetRequiredService<IOptions<BasketSettings>>().Value;

                var configuration = ConfigurationOptions.Parse(setting.RedisConnectionString, true);

                return ConnectionMultiplexer.Connect(configuration);
            });

            return services;
        }

        public static IServiceCollection AddIntegrationEventServices(this IServiceCollection services)
        {

            services.AddSingleton<IServiceBusConnection>(sp =>
            {
                var setting = sp.GetRequiredService<IOptions<BasketSettings>>().Value;

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
                var setting = sp.GetRequiredService<IOptions<BasketSettings>>().Value;

                return new ServiceBusEventBus(serviceBusConnection, logger,
                    subscriptionManager, sp, setting.SubscriptionClientName);
            });

            services.AddTransient<IIntegrationEventHandler<ProductPriceChangedIntegrationEvent>, ProductPriceChangedIntegrationEventHandler>();

            return services;
        }
    }
}
