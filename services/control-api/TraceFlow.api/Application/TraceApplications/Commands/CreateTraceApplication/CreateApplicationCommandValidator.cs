using FluentValidation;

namespace TraceFlow.Api.Application.TraceApplications.Commands.CreateTraceApplication;

public class CreateTraceApplicationCommandValidator
    : AbstractValidator<CreateTraceApplicationCommand>
{
    public CreateTraceApplicationCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Slug)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-zA-Z0-9-]+$")
            .WithMessage("Slug can only contain letters, numbers, and hyphens.");

        RuleFor(command => command.Description)
            .MaximumLength(500)
            .When(command => command.Description is not null);
    }
}