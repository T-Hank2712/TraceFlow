using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.Infrastructure.Persistence.Configurations;

public class TraceApplicationConfiguration
    : IEntityTypeConfiguration<TraceApplication>
{
    public void Configure(EntityTypeBuilder<TraceApplication> builder)
    {
        builder.ToTable("TraceApplications");

        builder.HasKey(application => application.Id);

        builder.Property(application => application.Id)
            .HasConversion(id => id.ToString(), value => Ulid.Parse(value))
            .IsRequired();

        builder.Property(application => application.ProjectId)
            .HasConversion(id => id.ToString(), value => Ulid.Parse(value))
            .IsRequired();

        builder.Property(application => application.CreatedByUserId)
            .HasConversion(id => id.ToString(), value => Ulid.Parse(value))
            .IsRequired();

        builder.Property(application => application.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(application => application.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(application => application.Description)
            .HasMaxLength(500);

        builder.Property(application => application.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(application => application.CreatedAt)
            .IsRequired();

        builder.Property(application => application.UpdatedAt)
            .IsRequired();

        builder.HasOne(application => application.Project)
            .WithMany(project => project.TraceApplications)
            .HasForeignKey(application => application.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(application => application.CreatedByUser)
            .WithMany()
            .HasForeignKey(application => application.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(application => new
            {
                application.ProjectId,
                application.Slug
            })
            .IsUnique()
            .HasFilter($"\"Status\" = '{ResourceStatuses.Active}'");
    }
}