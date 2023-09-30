using BasketAPI.IntegrationEvents.Events;
using EventBus.Contracts;

namespace BasketAPI.Extensions
{
    public static class AppExtensions
    {
        public static async Task<IApplicationBuilder> SubscribeEventsAsync(this IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

            await eventBus.SubscribeAsync<ProductPriceChangedIntegrationEvent, IIntegrationEventHandler<ProductPriceChangedIntegrationEvent>>();

            return app;
        }

        public static async Task<int> RunAppAsync(this WebApplication app)
        {
            try
            {
                app.Logger.LogInformation("Configuring web host ({AppName})...", Program.AppName);

                app.Logger.LogInformation("Starting web host ({AppName})...", Program.AppName);
                await app.RunAsync();

                return 0;
            }
            catch (Exception ex)
            {
                app.Logger.LogCritical(ex, "Program terminated unexpectedly ({AppName})!", Program.AppName);
                return 1;
            }
        }
    }
}
