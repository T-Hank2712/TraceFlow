using FluentValidation;
using System.Text.RegularExpressions;

namespace TraceFlow.Api.Application.Workspaces.Commands.UpdateWorkspace;
public class UpdateWorkspaceCommandValidator : AbstractValidator<UpdateWorkspaceCommand>
{
    public UpdateWorkspaceCommandValidator()
    {
        RuleFor(command => command)
            .Must(command =>
                command.Name is not null ||
                command.Slug is not null ||
                command.Description is not null)
            .WithMessage("At least one field must be provided.");

        RuleFor(command => command.Name)
            .MaximumLength(100)
            .When(command => command.Name is not null);

        RuleFor(command => command.Slug)
            .MaximumLength(80)
            .Must(slug => slug is null || Regex.IsMatch(slug.Trim(), "^[a-zA-Z0-9-]+$"))
            .WithMessage("Slug can only contain letters, numbers, and hyphens.")
            .When(command => command.Slug is not null);

        RuleFor(command => command.Description)
            .MaximumLength(500)
            .When(command => command.Description is not null);
    }
}