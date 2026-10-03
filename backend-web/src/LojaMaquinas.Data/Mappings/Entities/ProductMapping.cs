using LojaMaquinas.Business.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LojaMaquinas.Data.Mappings.Entities
{
    public class ProductMapping : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name).IsRequired().HasColumnType("varchar(30)");
            builder.Property(p => p.Brand).IsRequired().HasColumnType("varchar(30)");
            builder.Property(p => p.Model).IsRequired().HasColumnType("varchar(30)");
            builder.Property(m => m.Price).IsRequired().HasColumnType("decimal(6,2)");
            builder.Property(p => p.Description).IsRequired().HasColumnType("varchar(250)");

            builder.HasMany(s => s.Stocks).WithOne(p => p.Product).HasForeignKey(p => p.ProductId);

            builder.ToTable("E_Products");
        }
    }
}
