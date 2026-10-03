using LojaMaquinas.Business.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LojaMaquinas.Data.Mappings.Entities
    {
    public class MaintenanceMapping : IEntityTypeConfiguration<Maintenance>
        {
        public void Configure(EntityTypeBuilder<Maintenance> builder)
            {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Obs)
                .HasColumnType("varchar(250)");

            builder.Property(m => m.Price).IsRequired().HasColumnType("decimal(6,2)");

            builder.ToTable("E_Maintenances");
            }
        }
    }
