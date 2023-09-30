using EventBus.Contracts;
using EventBus.Events;

namespace CatalogAPI.IntegrationEvents
{
    public class CatalogIntegrationEventService
    {
        private readonly ILogger<CatalogIntegrationEventService> _logger;
        private readonly IEventBus _eventBus;

        public CatalogIntegrationEventService(ILogger<CatalogIntegrationEventService> logger, IEventBus eventBus) 
        {
            _logger = logger;
            _eventBus = eventBus;
        }

        public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
        {
            try
            {
                _logger.LogInformation("----- Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})", evt.Id, Program.AppName, evt);

                await _eventBus.PublishAsync(evt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})", evt.Id, Program.AppName, evt);
            }
        }
    }
}
