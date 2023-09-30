

namespace EventServiceBus
{
    public class DefaultServiceBusConnection : IServiceBusConnection
    {
        private readonly string _serviceBusConnectionString;
        private ServiceBusAdministrationClient _subscriptionClient;
        private ServiceBusClient _topicClient;

        bool _disposed;

        public DefaultServiceBusConnection(string serviceBusConnectionString)
        {
            _serviceBusConnectionString = serviceBusConnectionString;
            _subscriptionClient = new ServiceBusAdministrationClient(_serviceBusConnectionString);
            _topicClient = new ServiceBusClient(_serviceBusConnectionString);
        }

        public ServiceBusClient TopicClient
        {
            get
            {
                if (_topicClient.IsClosed)
                {
                    _topicClient = new ServiceBusClient(_serviceBusConnectionString);
                }
                return _topicClient;
            }    
        }

        public ServiceBusAdministrationClient AdministrationClient => _subscriptionClient;

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            _disposed = true;
            await _topicClient.DisposeAsync();
        }
    }
}
