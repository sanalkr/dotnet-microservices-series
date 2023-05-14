using CatalogAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogAPI.Data.EntityTypeConfigs
{
    public class CatalogItemEntityConfig :
        IEntityTypeConfiguration<CatalogItem>
    {
        public void Configure(EntityTypeBuilder<CatalogItem> builder)
        {
            builder.ToTable(nameof(CatalogItem));

            builder.HasKey(x => x.Id);

            builder.Property(ci => ci.Id).IsRequired();

            builder.Property(ci => ci.Name).IsRequired().HasMaxLength(50);

            builder.Property(ci => ci.Price).IsRequired();

            builder.Property(ci => ci.PictureFileName).IsRequired(false);

            builder.Ignore(ci => ci.PictureUri);

            builder.HasOne(ci => ci.CatalogBrand)
                .WithMany()
                .HasForeignKey(ci => ci.CatalogBrandId);

            builder.HasOne(ci => ci.CatalogType)
                .WithMany()
                .HasForeignKey(ci => ci.CatalogTypeId);
        }
    }
}
