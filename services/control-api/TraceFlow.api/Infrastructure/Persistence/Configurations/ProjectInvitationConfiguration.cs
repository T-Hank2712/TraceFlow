using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.Infrastructure.Persistence.Configurations;

public class ProjectInvitationConfiguration
    : IEntityTypeConfiguration<ProjectInvitation>
{
    public void Configure(EntityTypeBuilder<ProjectInvitation> builder)
    {
        builder.ToTable("ProjectInvitations");

        builder.HasKey(invitation => invitation.Id);

        builder.Property(invitation => invitation.Id)
            .HasConversion(id => id.ToString(), value => Ulid.Parse(value))
            .IsRequired();

        builder.Property(invitation => invitation.WorkspaceId)
            .HasConversion(id => id.ToString(), value => Ulid.Parse(value))
            .IsRequired();

        builder.Property(invitation => invitation.ProjectId)
            .HasConversion(id => id.ToString(), value => Ulid.Parse(value))
            .IsRequired();

        builder.Property(invitation => invitation.InvitedUserId)
            .HasConversion(id => id.ToString(), value => Ulid.Parse(value))
            .IsRequired();

        builder.Property(invitation => invitation.InvitedByUserId)
            .HasConversion(id => id.ToString(), value => Ulid.Parse(value))
            .IsRequired();

        builder.Property(invitation => invitation.Role)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(invitation => invitation.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(invitation => invitation.ExpiresAt)
            .IsRequired();

        builder.HasOne(invitation => invitation.Workspace)
            .WithMany()
            .HasForeignKey(invitation => invitation.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(invitation => invitation.Project)
            .WithMany()
            .HasForeignKey(invitation => invitation.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(invitation => invitation.InvitedUser)
            .WithMany()
            .HasForeignKey(invitation => invitation.InvitedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(invitation => invitation.InvitedByUser)
            .WithMany()
            .HasForeignKey(invitation => invitation.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(invitation => new
            {
                invitation.ProjectId,
                invitation.InvitedUserId
            })
            .IsUnique()
            .HasFilter("\"Status\" = 'pending'");
    }
}