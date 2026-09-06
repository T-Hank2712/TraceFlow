using FluentValidation;

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
            .Must(role => role is "admin" or "member")
            .WithMessage("Role must be either admin or member.");
    }
}