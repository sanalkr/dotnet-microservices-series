using CatalogAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogAPI.Data.EntityTypeConfigs
{
    public class CatalogTypeEntityConfig :
        IEntityTypeConfiguration<CatalogType>
    {
        public void Configure(EntityTypeBuilder<CatalogType> builder)
        {
            builder.ToTable(nameof(CatalogType));

            builder.HasKey(ct => ct.Id);

            builder.Property(ct => ct.Type).IsRequired().HasMaxLength(100);
        }
    }
}
