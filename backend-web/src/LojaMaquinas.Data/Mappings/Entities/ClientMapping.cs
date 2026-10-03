using LojaMaquinas.Business.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LojaMaquinas.Data.Mappings.Entities
    {
    public class ClientMapping : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
            {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.UserId)
                .IsRequired();

            builder.Property(c => c.FirstName)
                .IsRequired()
                .HasColumnType("varchar(20)");

            builder.Property(c => c.LastName)
                .IsRequired()
                .HasColumnType("varchar(20)");

            builder.Property(c => c.Address)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(c => c.PostalCode)
                .IsRequired()
                .HasColumnType("varchar(10)");

            builder.Property(c => c.PhoneNumber)
                .IsRequired()
                .HasColumnType("varchar(9)");

            builder.Property(c => c.NIF)
                .IsRequired()
                .HasColumnType("varchar(9)");

            builder.HasMany(o => o.Orders)
                   .WithOne(c => c.Client)
                   .HasForeignKey(c => c.ClientId)
                   .OnDelete(DeleteBehavior.ClientSetNull);

            builder.ToTable("E_Clients");
            }
        }
    }
