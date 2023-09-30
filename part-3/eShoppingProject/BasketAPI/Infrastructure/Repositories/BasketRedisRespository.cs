using BasketAPI.Infrastructure.Contracts;
using BasketAPI.Model;
using StackExchange.Redis;
using System.Text.Json;

namespace Basket.API.Infrastructure.Repositories
{
    public class BasketRedisRespository : IBasketRepository
    {
        private readonly ILogger<BasketRedisRespository> _logger;
        private readonly ConnectionMultiplexer _redisConnection;
        private readonly IDatabase _database;

        public BasketRedisRespository(ILogger<BasketRedisRespository> logger,
            ConnectionMultiplexer redisConnection)
        {
            _logger = logger;
            _redisConnection = redisConnection;
            _database = redisConnection.GetDatabase();
        }
        public async Task<bool> DeleteBasketAsync(string id)
        {
            return await _database.KeyDeleteAsync(id);
        }

        public async Task<CustomerBasket> GetBasketAsync(string customerId)
        {
            var data = await _database.StringGetAsync(customerId);

            if (data.IsNullOrEmpty)
            {
                return null;
            }

            return JsonSerializer.Deserialize<CustomerBasket>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public IEnumerable<string> GetUsers()
        {
            var server = GetServer();
            var data = server.Keys();

            return data?.Select(k => k.ToString());
        }

        public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket)
        {
            var created = await _database.StringSetAsync(basket.BuyerId, JsonSerializer.Serialize(basket));

            if (!created)
            {
                _logger.LogInformation("Unable to update the item");
                return null;
            }

            _logger.LogInformation($"Basket item created successfully");

            return await GetBasketAsync(basket.BuyerId);
        }

        private IServer GetServer()
        {
            var endpoint = _redisConnection.GetEndPoints();
            return _redisConnection.GetServer(endpoint.First());
        }
    }
}
