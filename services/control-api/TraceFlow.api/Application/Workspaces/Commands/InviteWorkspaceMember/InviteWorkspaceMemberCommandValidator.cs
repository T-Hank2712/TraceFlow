using FluentValidation;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Application.Workspaces.Commands.InviteWorkspaceMember;

public class InviteWorkspaceMemberCommandValidator
    : AbstractValidator<InviteWorkspaceMemberCommand>
{
    public InviteWorkspaceMemberCommandValidator()
    {
        RuleFor(command => command.Identifier)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Role)
            .NotEmpty()
            .Must(role => WorkspaceMemberRoles.Assignable.Contains(role.Trim().ToLowerInvariant()))
            .WithMessage("Role must be either admin or member.");
    }
}