using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.Infrastructure.Persistence.Configurations;

public class ProjectMemberConfiguration
    : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.ToTable("ProjectMembers");

        builder.HasKey(member => member.Id);

        builder.Property(member => member.Id)
            .HasConversion(id => id.ToString(), value => Ulid.Parse(value))
            .IsRequired();

        builder.Property(member => member.ProjectId)
            .HasConversion(id => id.ToString(), value => Ulid.Parse(value))
            .IsRequired();

        builder.Property(member => member.UserId)
            .HasConversion(id => id.ToString(), value => Ulid.Parse(value))
            .IsRequired();

        builder.Property(member => member.Role)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(member => member.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(member => member.JoinedAt)
            .IsRequired();

        builder.Property(member => member.CreatedAt)
            .IsRequired();

        builder.Property(member => member.UpdatedAt)
            .IsRequired();

        builder.HasOne(member => member.Project)
            .WithMany(project => project.Members)
            .HasForeignKey(member => member.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(member => member.User)
            .WithMany(user => user.ProjectMemberships)
            .HasForeignKey(member => member.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(member => new
            {
                member.ProjectId,
                member.UserId
            })
            .IsUnique();
    }
}