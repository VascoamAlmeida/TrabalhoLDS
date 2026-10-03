using LojaMaquinas.Business.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LojaMaquinas.Data.Mappings.Entities
{
    public class OrderRentMapping : IEntityTypeConfiguration<OrderRent>
    {
        public void Configure(EntityTypeBuilder<OrderRent> builder)
        {
            builder.HasKey(o => o.Id);

            // Relationship: Order (1) - (0..1) OrderRent
            // One Order may have one OrderRent
            builder.HasOne(c => c.Contract)
                   .WithOne(o => o.OrderRent)
                   .OnDelete(DeleteBehavior.ClientSetNull);

            // Database table name
            builder.ToTable("E_OrderRents");
        }
    }
}
