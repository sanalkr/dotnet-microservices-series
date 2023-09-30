using BasketAPI.Infrastructure.Contracts;
using BasketAPI.IntegrationEvents.Events;
using BasketAPI.Model;
using EventBus.Contracts;
using Serilog.Context;

namespace BasketAPI.IntegrationEvents.Handlers
{
    public class ProductPriceChangedIntegrationEventHandler : IIntegrationEventHandler<ProductPriceChangedIntegrationEvent>
    {
        private readonly ILogger<ProductPriceChangedIntegrationEventHandler> _logger;
        private readonly IBasketRepository _repository;

        public ProductPriceChangedIntegrationEventHandler(
            ILogger<ProductPriceChangedIntegrationEventHandler> logger,
            IBasketRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task Handle(ProductPriceChangedIntegrationEvent @event)
        {
            using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
            {
                _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

                IEnumerable<string> userIds = _repository.GetUsers();

                foreach (string userId in userIds)
                {
                    var basket = await _repository.GetBasketAsync(userId);

                    await UpdatePriceInBasketItems(@event.ProductId, @event.NewPrice, @event.OldPrice, basket);
                }
            }
        }

        private async Task UpdatePriceInBasketItems(int productId, decimal newPrice, decimal oldPrice, CustomerBasket basket)
        {
            var itemsToUpdate = basket.Items?.Where(x => x.ProductId == productId).ToList();

            if (itemsToUpdate != null)
            {
                itemsToUpdate.ForEach((item) =>
                {
                    if (item.UnitPrice == oldPrice)
                    {
                        var orgPrice = item.UnitPrice;
                        item.UnitPrice = newPrice;
                        item.OldUnitPrice = orgPrice;
                    }
                });
                await _repository.UpdateBasketAsync(basket);
            }
        }
    }
}
