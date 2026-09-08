using FluentValidation;

namespace TraceFlow.Api.Application.Projects.Commands.UpdateProject;

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
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
            .MaximumLength(100)
            .Matches("^[a-zA-Z0-9-]+$")
            .WithMessage("Slug can only contain letters, numbers, and hyphens.")
            .When(command => command.Slug is not null);

        RuleFor(command => command.Description)
            .MaximumLength(500)
            .When(command => command.Description is not null);
    }
}