using FluentValidation;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Application.Projects.Commands.InviteProjectMember;

public class InviteProjectMemberCommandValidator
    : AbstractValidator<InviteProjectMemberCommand>
{
    public InviteProjectMemberCommandValidator()
    {
        RuleFor(command => command.Identifier)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Role)
            .NotEmpty()
            .Must(role => ProjectMemberRoles.All.Contains(
                role.Trim().ToLowerInvariant()))
            .WithMessage("Role must be manager, developer, or viewer.");
    }
}