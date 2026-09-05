using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).HasConversion(id => id.ToString(), value => Ulid.Parse(value)).IsRequired();

        builder.Property(user => user.Email).IsRequired().HasMaxLength(100);
        builder.HasIndex(user => user.Email).IsUnique();
        builder.Property(user => user.UserName).IsRequired().HasMaxLength(50);
        builder.HasIndex(user => user.UserName).IsUnique();
        builder.Property(user => user.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(user => user.LastName).IsRequired().HasMaxLength(100);
        builder.Property(user => user.PasswordHash).IsRequired();
        builder.Property(user => user.Role).IsRequired().HasMaxLength(20);
        builder.Property(user => user.Status).IsRequired().HasMaxLength(20);
        builder.Property(user => user.UpdatedAt).IsRequired();
        builder.Property(user => user.CreatedAt).IsRequired();
    }
}