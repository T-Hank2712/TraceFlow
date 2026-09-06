using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.Infrastructure.Persistence.Configurations;

public class WorkspaceMemberConfiguration
    : IEntityTypeConfiguration<WorkspaceMember>
{
    public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
    {
        builder.ToTable("WorkspaceMembers");

        builder.HasKey(member => member.Id);

        builder.Property(member => member.Id)
            .HasConversion(
                id => id.ToString(),
                value => Ulid.Parse(value))
            .IsRequired();

        builder.Property(member => member.WorkspaceId)
            .HasConversion(
                id => id.ToString(),
                value => Ulid.Parse(value))
            .IsRequired();

        builder.Property(member => member.UserId)
            .HasConversion(
                id => id.ToString(),
                value => Ulid.Parse(value))
            .IsRequired();

        builder.HasOne(member => member.Workspace)
            .WithMany(workspace => workspace.Members)
            .HasForeignKey(member => member.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(member => member.User)
            .WithMany(user => user.WorkspaceMemberships)
            .HasForeignKey(member => member.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(member => new
        {
            member.WorkspaceId,
            member.UserId
        }).IsUnique();

        builder.HasIndex(member => member.UserId);

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
    }
}