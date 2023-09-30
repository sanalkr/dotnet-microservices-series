using BasketAPI.Infrastructure.Contracts;
using BasketAPI.Model;
using BasketAPI.Services;
using EventBus.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace BasketAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly ILogger<BasketController> _logger;
        private readonly IBasketRepository _repository;
        private readonly IIdentityService _identityService;
        //private readonly IEventBus _eventBus;

        public BasketController(ILogger<BasketController> logger,
            IBasketRepository repository, IIdentityService identityService/*,
            IEventBus eventBus*/)
        {
            _logger = logger;
            _repository = repository;
            _identityService = identityService;
            //_eventBus = eventBus;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<CustomerBasket>> GetBasketByIdAsync(string id)
        {
            var basket = await _repository.GetBasketAsync(id);

            return Ok(basket ?? new CustomerBasket(id));
        }

        [HttpPost]
        [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<CustomerBasket>> UpdateBasketAsync([FromBody] CustomerBasket value)
        {
            return Ok(await _repository.UpdateBasketAsync(value));
        }

        [Route("checkout")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> CheckoutAsync([FromBody] BasketCheckout basketCheckout, [FromHeader(Name = "x-requestid")] string requestId)
        {
            var userId = _identityService.GetUserIdentity();

            basketCheckout.RequestId = (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty) ?
                guid : basketCheckout.RequestId;

            var basket = await _repository.GetBasketAsync(userId);

            if (basket == null)
            {
                return BadRequest();
            }
            var userName = this.HttpContext.User.FindFirst(x => x.Type == ClaimTypes.Name).Value;

            //var eventMessage = new UserCheckoutIntegrationEvent(userId, userName, basketCheckout, basket);

            //try
            //{
            //    await _eventBus.PublishAsync(eventMessage);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "ERROR Publishing integration event: {IntegrationEventId} from {AppName}", eventMessage.Id, Program.AppName);

            //    throw;
            //}

            return Accepted();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task DeleteBasketByIdAsync(string id)
        {
            await _repository.DeleteBasketAsync(id);
        }
    }
}