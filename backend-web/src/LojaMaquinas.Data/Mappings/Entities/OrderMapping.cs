using LojaMaquinas.Business.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LojaMaquinas.Data.Mappings.Entities
{
    public class OrderMapping : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(m => m.Total).IsRequired().HasColumnType("decimal(6,2)");

            // Relationship: Order (1) - (0..1) OrderRent
            // One Order may have one OrderRent
            builder.HasOne(o => o.OrderRent)
                .WithOne(or => or.Order)
                .HasForeignKey<OrderRent>(or => or.OrderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Relationship: Order (1) - (0..*) Maintenance
            builder.HasMany(o => o.Maintenances)
                .WithOne(m => m.Order)
                .HasForeignKey(m => m.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Database table name
            builder.ToTable("E_Orders");
        }
    }
}
