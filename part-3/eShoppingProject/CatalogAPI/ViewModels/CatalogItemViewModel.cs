namespace Catalog.API.ViewModel
{
    public class CatalogItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string PictureFileName { get; set; }
        public string PictureUri { get; set; }
        public string CatalogType { get; set; }
        public string CatalogBrand { get; set; }
    }
}
