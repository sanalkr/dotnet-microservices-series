using Asp.Versioning;
using Catalog.API.ViewModel;
using CatalogAPI.Data;
using CatalogAPI.Extensions;
using CatalogAPI.Models;
using CatalogAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace CatalogAPI.Controllers
{
    [ApiController]
    [ApiVersion(1.0)]
    [ApiVersion(2.0)]
    [Route("api/[controller]")]
    //[Route("api/v{version:apiVersion}/[controller]")] URI Versioning
    public class CatalogController : ControllerBase
    {   
        private readonly ILogger<CatalogController> _logger;
        private readonly CatalogContext _context;

        public CatalogController(ILogger<CatalogController> logger,
            CatalogContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [Route("getitems"), MapToApiVersion(1.0)]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IEnumerable<CatalogItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetItemsAsync(
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageIndex = 0,
            string ids = null)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var items = await GetItemsByIdsAsync(ids);
                if (!items.Any())
                {
                    return BadRequest("ids value is invalid. Must be a comma separated value");
                }

                return Ok(items);
            }

            var totalItems = await _context.CatalogItems
                .LongCountAsync();

            var pageItems = await _context.CatalogItems
                .OrderBy(x => x.Name)
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, pageItems));
        }

        [HttpGet]
        [Route("getitems"), MapToApiVersion(2.0)]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItemViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IEnumerable<CatalogItemViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetItemsAsyncV2(
            [FromQuery]int pageSize = 10,
            [FromQuery]int pageIndex = 0,
            string ids = null)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var items = await GetItemsByIdsAsync(ids);
                if (!items.Any())
                {
                    return BadRequest("ids value is invalid. Must be a comma separated value");
                }

                return Ok(items.ToViewModel());
            }

            var totalItems = await _context.CatalogItems
                .LongCountAsync();

            var pageItems = await _context.CatalogItems
                .OrderBy(x => x.Name)
                .Include(x => x.CatalogBrand)
                .Include(x => x.CatalogType)
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PaginatedItemsViewModel<CatalogItemViewModel>(pageIndex, pageSize, totalItems, pageItems.ToViewModel()));
        }

        private async Task<List<CatalogItem>> GetItemsByIdsAsync(string ids)
        {
            var numIds = ids.Split(",").Select(id => (Valid: int.TryParse(id, out int x), Value: x));

            if(!numIds.All(id => id.Valid))
            {
                return new List<CatalogItem>();
            }

            var idsSelect = numIds.Select(id => id.Value);
            var items = await _context.CatalogItems.Where(ci => idsSelect.Contains(ci.Id))
                .Include(x => x.CatalogBrand)
                .Include(x => x.CatalogType)
                .ToListAsync();

            return items;
        }

        [HttpGet]
        [Route("getitem/{id:int}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(CatalogItem), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<CatalogItem>> GetItemByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var item = await _context.CatalogItems.SingleOrDefaultAsync(ci => ci.Id == id);

            if (item != null)
            {
                return item;
            }

            return NotFound();
        }

        [Route("updateitem")]
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> UpdateProductAsync([FromBody] CatalogItem productToUpdate)
        {
            var catalogItem = await _context.CatalogItems.SingleOrDefaultAsync(i => i.Id == productToUpdate.Id);

            if (catalogItem == null)
            {
                return NotFound(new { Message = $"Item with id {productToUpdate.Id} not found." });
            }

            var oldPrice = catalogItem.Price;
            var raiseProductPriceChangedEvent = oldPrice != productToUpdate.Price;

            // Update current product
            catalogItem = productToUpdate;
            _context.CatalogItems.Update(catalogItem);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItemByIdAsync), new { id = productToUpdate.Id }, null);
        }
       
        [Route("additem")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> CreateProductAsync([FromBody] CatalogItem product)
        {
            var item = new CatalogItem
            {
                CatalogBrandId = product.CatalogBrandId,
                CatalogTypeId = product.CatalogTypeId,
                Description = product.Description,
                Name = product.Name,
                PictureFileName = product.PictureFileName,
                Price = product.Price
            };

            _context.CatalogItems.Add(item);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItemByIdAsync), new { id = item.Id }, null);
        }
        
        [Route("deleteitem/{id}")]
        [HttpDelete]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> DeleteProductAsync(int id)
        {
            var product = _context.CatalogItems.SingleOrDefault(x => x.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            _context.CatalogItems.Remove(product);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}