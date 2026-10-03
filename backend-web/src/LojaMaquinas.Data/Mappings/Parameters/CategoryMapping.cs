using LojaMaquinas.Business.Models.Parameters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LojaMaquinas.Data.Mappings.Parameters
{
    public class CategoryMapping : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).IsRequired().HasColumnType("varchar(30)");

            builder.Property(c => c.Description).IsRequired().HasColumnType("varchar(250)");

            //Relacionamento 1:N->Category tem muitos Products
            builder.HasMany(p => p.Products)
                    .WithOne(u => u.Category)
                    .HasForeignKey(u => u.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

            builder.ToTable("P_Categories");
        }
    }
}
