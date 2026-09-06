using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.Infrastructure.Persistence.Configurations;

public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("Workspaces");

        builder.HasKey(workspace => workspace.Id);

        builder.Property(workspace => workspace.Id)
            .HasConversion(
                id => id.ToString(),
                value => Ulid.Parse(value))
            .IsRequired();

        builder.Property(workspace => workspace.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(workspace => workspace.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(workspace => workspace.Slug)
            .IsUnique();

        builder.Property(workspace => workspace.Description)
            .HasMaxLength(500);

        builder.Property(workspace => workspace.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(workspace => workspace.CreatedAt)
            .IsRequired();

        builder.Property(workspace => workspace.UpdatedAt)
            .IsRequired();
    }
}