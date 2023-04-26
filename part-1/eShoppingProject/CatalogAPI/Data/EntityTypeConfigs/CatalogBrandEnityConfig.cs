using CatalogAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogAPI.Data.EntityTypeConfigs
{
    public class CatalogBrandEnityConfig :
        IEntityTypeConfiguration<CatalogBrand>
    {
        public void Configure(EntityTypeBuilder<CatalogBrand> builder)
        {
            builder.ToTable(nameof(CatalogBrand));

            builder.HasKey(cb => cb.Id);
            
            builder.Property(cb => cb.Brand).HasMaxLength(50);
        }
    }
}
