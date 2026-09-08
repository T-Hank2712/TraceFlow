using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(project => project.Id);

        builder.Property(project => project.Id)
            .HasConversion(id => id.ToString(), value => Ulid.Parse(value))
            .IsRequired();

        builder.Property(project => project.WorkspaceId)
            .HasConversion(id => id.ToString(), value => Ulid.Parse(value))
            .IsRequired();

        builder.Property(project => project.CreatedByUserId)
            .HasConversion(id => id.ToString(), value => Ulid.Parse(value))
            .IsRequired();

        builder.Property(project => project.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(project => project.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(project => project.Description)
            .HasMaxLength(500);

        builder.Property(project => project.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne(project => project.Workspace)
            .WithMany(workspace => workspace.Projects)
            .HasForeignKey(project => project.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(project => project.CreatedByUser)
            .WithMany()
            .HasForeignKey(project => project.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(project => new
            {
                project.WorkspaceId,
                project.Slug
            })
            .IsUnique()
            .HasFilter($"\"Status\" = '{ResourceStatuses.Active}'");
    }
}