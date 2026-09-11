using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.Infrastructure.Persistence.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("ApiKeys");

        builder.HasKey(apiKey => apiKey.Id);

        builder.Property(apiKey => apiKey.Id)
            .HasConversion(
                id => id.ToString(),
                value => Ulid.Parse(value));

        builder.Property(apiKey => apiKey.TraceApplicationId)
            .HasConversion(
                id => id.ToString(),
                value => Ulid.Parse(value))
            .IsRequired();

        builder.Property(apiKey => apiKey.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(apiKey => apiKey.KeyPrefix)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(apiKey => apiKey.SecretHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(apiKey => apiKey.Status)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(apiKey => apiKey.CreatedAt)
            .IsRequired();

        builder.Property(apiKey => apiKey.UpdatedAt)
            .IsRequired();

        builder.Property(apiKey => apiKey.ExpiresAt)
            .IsRequired();

        builder.Property(apiKey => apiKey.RevokedAt);

        builder.Property(apiKey => apiKey.LastUsedAt);

        builder.HasOne(apiKey => apiKey.TraceApplication)
            .WithMany(application => application.ApiKeys)
            .HasForeignKey(apiKey => apiKey.TraceApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(apiKey => apiKey.TraceApplicationId);

        builder.HasIndex(apiKey => apiKey.KeyPrefix)
            .IsUnique();
    }
}