using FluentValidation;

namespace TraceFlow.Api.Application.Users.Commands.UpdateProfile;

public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.UserName)
            .MaximumLength(50)
            .When(command => command.UserName is not null);

        RuleFor(command => command.FirstName)
            .MaximumLength(100)
            .When(command => command.FirstName is not null);

        RuleFor(command => command.LastName)
            .MaximumLength(100)
            .When(command => command.LastName is not null);

        RuleFor(command => command)
            .Must(command =>
                command.UserName is not null ||
                command.FirstName is not null ||
                command.LastName is not null)
            .WithMessage("At least one profile field must be provided.");
    }
}