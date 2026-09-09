using FluentValidation;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Application.Projects.Commands.ChangeProjectMemberRole;

public class ChangeProjectMemberRoleCommandValidator
    : AbstractValidator<ChangeProjectMemberRoleCommand>
{
    public ChangeProjectMemberRoleCommandValidator()
    {
        RuleFor(command => command.Role)
            .NotEmpty()
            .Must(role => ProjectMemberRoles.All.Contains(
                role.Trim().ToLowerInvariant()))
            .WithMessage("Role must be manager, developer, or viewer.");
    }
}