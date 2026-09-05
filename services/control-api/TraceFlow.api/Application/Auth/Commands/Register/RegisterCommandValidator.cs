using FluentValidation;

namespace TraceFlow.Api.Application.Auth.Commands.Register;
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command => command.Email)
        .NotEmpty()
        .EmailAddress()
        .MaximumLength(100);

        RuleFor(command => command.UserName)
        .NotEmpty()
        .MaximumLength(50);

        RuleFor(command => command.FirstName)
        .NotEmpty()
        .MaximumLength(100);

        RuleFor(command => command.LastName)
        .NotEmpty()
        .MaximumLength(100);

        RuleFor(command => command.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100)
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]")
            .WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one number.")
            .Matches(@"[\W_]")
            .WithMessage("Password must contain at least one special character.");

        RuleFor(command => command.ConfirmPassword)
            .NotEmpty()
            .Equal(command => command.Password)
            .WithMessage("Passwords do not match.");
    }
}