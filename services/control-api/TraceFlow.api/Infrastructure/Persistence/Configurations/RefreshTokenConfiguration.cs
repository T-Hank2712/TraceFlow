using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.Infrastructure.Persistence.Configurations;
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(token => token.Id);
        builder.Property(token => token.Id).HasConversion(id => id.ToString(), value => Ulid.Parse(value)).IsRequired();

        builder.Property(token => token.UserId).HasConversion(id => id.ToString(), value => Ulid.Parse(value)).IsRequired();
        builder.Property(token => token.UserId)
            .HasConversion(id => id.ToString(), value => Ulid.Parse(value))
            .IsRequired();

        builder.HasOne(token => token.User)
            .WithMany(user => user.RefreshTokens)
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(token => token.TokenHash).IsRequired().HasMaxLength(100);
        builder.HasIndex(token => token.TokenHash).IsUnique();
        builder.Property(token => token.ExpiresAt).IsRequired();
        builder.Property(token => token.RevokedAt);
        builder.Property(token => token.UpdatedAt).IsRequired();
        builder.Property(token => token.CreatedAt).IsRequired();
    }
}