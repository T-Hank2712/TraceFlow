using FluentValidation;

namespace TraceFlow.Api.Application.Workspaces.Commands.CreateWorkspace;

public class CreateWorkspaceCommandValidator
    : AbstractValidator<CreateWorkspaceCommand>
{
    public CreateWorkspaceCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Slug)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Slug can only contain lowercase letters, numbers, and hyphens.");

        RuleFor(command => command.Description)
            .MaximumLength(500)
            .When(command => command.Description is not null);
    }
}