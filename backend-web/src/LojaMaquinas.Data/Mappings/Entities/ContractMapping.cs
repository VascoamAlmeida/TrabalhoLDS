using LojaMaquinas.Business.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LojaMaquinas.Data.Mappings.Entities
{
    public class ContractMapping : IEntityTypeConfiguration<Contract>
    {
        public void Configure(EntityTypeBuilder<Contract> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.TermsConditions).IsRequired().HasColumnType("varchar(250)");

            // Database table name
            builder.ToTable("E_Contracts");
        }
    }
}
