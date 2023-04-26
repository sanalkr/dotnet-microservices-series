using CatalogAPI.Data;
using CatalogAPI.Models;
using CatalogAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        [Route("getitems")]
        public async Task<IActionResult> GetItemsAsync(
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

        private async Task<List<CatalogItem>> GetItemsByIdsAsync(string ids)
        {
            var numIds = ids.Split(",").Select(id => (Valid: int.TryParse(id, out int x), Value: x));

            if(!numIds.All(id => id.Valid))
            {
                return new List<CatalogItem>();
            }

            var idsSelect = numIds.Select(id => id.Value);
            var items = await _context.CatalogItems.Where(ci => idsSelect.Contains(ci.Id)).ToListAsync();

            return items;
        }
    }
}