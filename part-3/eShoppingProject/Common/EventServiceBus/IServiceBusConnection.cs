

namespace EventServiceBus
{
    public interface IServiceBusConnection : IAsyncDisposable
    {
        ServiceBusClient TopicClient { get; }
        ServiceBusAdministrationClient AdministrationClient { get; }
    }
}