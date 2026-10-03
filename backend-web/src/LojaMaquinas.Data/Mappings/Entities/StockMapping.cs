using LojaMaquinas.Business.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LojaMaquinas.Data.Mappings.Entities
{
    public class StockMapping : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.HasKey(s => s.Id);

            // 1 : M => Stock : Order
            builder.HasMany(o => o.Orders).WithOne(s => s.Stock).HasForeignKey(s => s.StockId);

            builder.ToTable("E_Stocks");
        }
    }
}
