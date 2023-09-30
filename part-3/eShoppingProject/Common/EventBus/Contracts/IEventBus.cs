

namespace EventBus.Contracts
{
    public interface IEventBus
    {
        Task PublishAsync(IntegrationEvent @event);

        Task SubscribeAsync<T, TH>() 
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        Task UnsubscribeAsync<T, TH>()
            where T: IntegrationEvent
            where TH: IIntegrationEventHandler<T>;

    }
}
