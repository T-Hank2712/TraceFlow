using FluentValidation;

namespace TraceFlow.Api.Application.Workspaces.Commands.ChangeMemberRole;

public class ChangeMemberRoleCommandValidator
    : AbstractValidator<ChangeMemberRoleCommand>
{
    public ChangeMemberRoleCommandValidator()
    {
        RuleFor(command => command.Role)
            .NotEmpty()
            .Must(role => role.Trim().ToLowerInvariant() is "admin" or "member")
            .WithMessage("Role must be either admin or member.");
    }
}