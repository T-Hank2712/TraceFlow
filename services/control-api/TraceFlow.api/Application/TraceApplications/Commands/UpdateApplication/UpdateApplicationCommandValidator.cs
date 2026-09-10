using FluentValidation;

namespace TraceFlow.Api.Application.TraceApplications.Commands.UpdateApplication;

public class UpdateApplicationCommandValidator : AbstractValidator<UpdateApplicationCommand>
{
    public UpdateApplicationCommandValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => x.Name is not null);

        RuleFor(x => x.Slug)
            .MaximumLength(100)
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Slug can only contain lowercase letters, numbers, and hyphens.")
            .When(x => x.Slug is not null);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null);

        RuleFor(x => x)
            .Must(x =>
                x.Name is not null ||
                x.Slug is not null ||
                x.Description is not null)
            .WithMessage("At least one field must be provided.");
    }
}