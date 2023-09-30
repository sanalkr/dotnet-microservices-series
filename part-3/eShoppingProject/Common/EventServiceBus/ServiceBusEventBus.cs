

namespace EventServiceBus
{
    public class ServiceBusEventBus : IEventBus, IAsyncDisposable
    {
        private readonly IServiceBusConnection _serviceBusConnection;
        private readonly ILogger<ServiceBusEventBus> _logger;
        private readonly IEventBusSubscriptionManager _subscriptionManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _subscriptionName;
        private readonly ServiceBusSender _sender;
        private readonly string _topicName = "eshop_event_bus";
        private readonly ServiceBusProcessor _processor;
        private const string INTEGRATION_EVENT_SUFFIX = "IntegrationEvent";

        public ServiceBusEventBus(IServiceBusConnection serviceBusConnection,
            ILogger<ServiceBusEventBus> logger, IEventBusSubscriptionManager subscriptionManager,
            IServiceProvider serviceProvider, string subscriptionClientName)
        {
            _serviceBusConnection = serviceBusConnection;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subscriptionManager = subscriptionManager ?? new InMemoryEventBusSubscriptionsManager();
            _serviceProvider = serviceProvider;
            _subscriptionName = subscriptionClientName;
            _sender = _serviceBusConnection.TopicClient.CreateSender(_topicName);
            ServiceBusProcessorOptions options = new ServiceBusProcessorOptions { MaxConcurrentCalls = 10, AutoCompleteMessages = false };
            _processor = _serviceBusConnection.TopicClient.CreateProcessor(_topicName, _subscriptionName, options);

            RemoveDefaultRule();
            RegisterSubscriptionMessageHanlderAsync().GetAwaiter().GetResult();
        }

        private async Task RegisterSubscriptionMessageHanlderAsync()
        {
            _processor.ProcessMessageAsync +=
                async (args) =>
                {
                    var eventName = $"{args.Message.Subject}{INTEGRATION_EVENT_SUFFIX}";
                    string messageData = args.Message.Body.ToString();

                    if(await ProcessEvent(eventName, messageData))
                    {
                        await args.CompleteMessageAsync(args.Message);
                    }
                };

            _processor.ProcessErrorAsync += ErrorHandler;
            await _processor.StartProcessingAsync();
        }

        private Task ErrorHandler(ProcessErrorEventArgs arg)
        {
            var ex = arg.Exception;
            var context = arg.ErrorSource;

            _logger.LogError(ex, "ERROR handling message: {ExceptionMessage} - Context: {@ExceptionContext}", ex.Message, context);

            return Task.CompletedTask;
        }

        private async Task<bool> ProcessEvent(string eventName, string message)
        {
            var processed = false;
            if (_subscriptionManager.HasSubscriptionsForEvent(eventName))
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                var subscriptions = _subscriptionManager.GetHandlersForEvent(eventName);
                foreach( var subscription in subscriptions)
                {
                    try
                    {
                        var handler = scope.ServiceProvider.GetService(subscription.HandlerType);
                        if (handler == null) continue;

                        var eventType = _subscriptionManager.GetEventTypeByName(eventName);
                        var integrationEvent = JsonSerializer.Deserialize(message, eventType);
                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new[] { integrationEvent });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occured");
                        //throw;
                    }
                }                        
            }

            processed = true;
            return processed;
        }

        private void RemoveDefaultRule()
        {
            try
            {
                _serviceBusConnection
                    .AdministrationClient
                    .DeleteRuleAsync(_topicName, _subscriptionName, RuleProperties.DefaultRuleName)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
            {
                _logger.LogWarning("The messaging rile {DefaultRuleName} could not be found", RuleProperties.DefaultRuleName);
            }
        }

        public async Task PublishAsync(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name.Replace(INTEGRATION_EVENT_SUFFIX, "");
            var jsonMessage = JsonSerializer.Serialize(@event, @event.GetType());
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var message = new ServiceBusMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = new BinaryData(body),
                Subject = eventName
            };

            //_sender.SendMessageAsync(message)
            //    .GetAwaiter().GetResult();
            await _sender.SendMessageAsync(message);
        }

        public async Task SubscribeAsync<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name.Replace(INTEGRATION_EVENT_SUFFIX, "");

            var hasEvent = _subscriptionManager.HasSubscriptionsForEvent<T>();
            if(!hasEvent)
            {
                try
                {
                    await _serviceBusConnection.AdministrationClient.CreateRuleAsync(_topicName, _subscriptionName, new CreateRuleOptions
                    {
                        Filter = new CorrelationRuleFilter() { Subject = eventName },
                        Name = eventName
                    });
                }
                catch (ServiceBusException ex)
                {
                    _logger.LogWarning(ex, "The messaging entity {eventName} already exists.", eventName);
                }
            }

            _subscriptionManager.AddSubscription<T, TH>();
        }

        public async Task UnsubscribeAsync<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name.Replace(INTEGRATION_EVENT_SUFFIX, "");

            try
            {
                await _serviceBusConnection
                    .AdministrationClient
                    .DeleteRuleAsync(_topicName, _subscriptionName, eventName);
                    //.GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                _logger.LogWarning("The messaging entity {eventName} Could not be found.", eventName);
            }

            _logger.LogInformation("Unsubscribing from event {EventName}", eventName);

            _subscriptionManager.RemoveSubscription<T, TH>();
        }

        public async ValueTask DisposeAsync()
        {
            _subscriptionManager.Clear();
            await _processor.CloseAsync();
        }
    }
}
