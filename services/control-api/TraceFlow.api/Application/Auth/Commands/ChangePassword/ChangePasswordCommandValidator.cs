using FluentValidation;

namespace TraceFlow.Api.Application.Auth.Commands.ChangePassword;

public class ChangePasswordCommandValidator
    : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.CurrentPassword)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.NewPassword)
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

        RuleFor(command => command.ConfirmNewPassword)
            .NotEmpty()
            .Equal(command => command.NewPassword)
            .WithMessage("Passwords do not match.");

        RuleFor(command => command.NewPassword)
            .NotEqual(command => command.CurrentPassword)
            .WithMessage("New password must be different from current password.");
    }
}