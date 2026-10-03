using LojaMaquinas.Business.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LojaMaquinas.Data.Mappings.Entities
    {
    public class UserMapping : IEntityTypeConfiguration<User>
        {
        public void Configure(EntityTypeBuilder<User> builder)
            {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(u => u.Password)
                .IsRequired()
                .HasColumnType("varchar(250)");

            builder.Property(u => u.UserType)
                .IsRequired()
                .HasColumnType("varchar(20)");

            // Relacionamento 1:1 -> User tem um Client
            builder.HasOne(u => u.Client)
                    .WithOne(c => c.User)
                    .HasForeignKey<Client>(c => c.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

            builder.ToTable("E_Users");
            }
        }
    }
