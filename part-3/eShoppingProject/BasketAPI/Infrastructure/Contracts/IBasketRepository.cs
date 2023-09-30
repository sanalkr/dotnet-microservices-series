using BasketAPI.Model;

namespace BasketAPI.Infrastructure.Contracts
{
    public interface IBasketRepository
    {
        Task<bool> DeleteBasketAsync(string id);
        Task<CustomerBasket> GetBasketAsync(string id);
        IEnumerable<string> GetUsers();
        Task<CustomerBasket> UpdateBasketAsync(CustomerBasket value);
    }
}
