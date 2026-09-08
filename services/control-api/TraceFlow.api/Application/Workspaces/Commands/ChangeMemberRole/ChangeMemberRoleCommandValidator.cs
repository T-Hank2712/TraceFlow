using FluentValidation;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Application.Workspaces.Commands.ChangeMemberRole;

public class ChangeMemberRoleCommandValidator
    : AbstractValidator<ChangeMemberRoleCommand>
{
    public ChangeMemberRoleCommandValidator()
    {
        RuleFor(command => command.Role)
            .NotEmpty()
            .Must(role => WorkspaceMemberRoles.Assignable.Contains(role.Trim().ToLowerInvariant()))
            .WithMessage("Role must be either admin or member.");
    }
}