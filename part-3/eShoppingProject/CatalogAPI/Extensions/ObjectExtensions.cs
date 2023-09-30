using Catalog.API.ViewModel;
using CatalogAPI.Models;

namespace CatalogAPI.Extensions
{
    public static class ObjectExtensions
    {
        public static CatalogItemViewModel ToViewModel(this CatalogItem catalogItem)
        {
            return new CatalogItemViewModel
            {
                Id = catalogItem.Id,
                Name = catalogItem.Name,
                Description = catalogItem.Description,
                PictureFileName = catalogItem.PictureFileName,
                PictureUri = catalogItem.PictureUri,
                Price = catalogItem.Price,
                CatalogBrand = catalogItem.CatalogBrand.Brand,
                CatalogType = catalogItem.CatalogType.Type
            };
        }

        public static IEnumerable<CatalogItemViewModel> ToViewModel(this IEnumerable<CatalogItem> catalogItems)
        {
            foreach (var item in catalogItems)
            {
                yield return item.ToViewModel();
            }
        }
    }
}
