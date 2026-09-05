using FluentValidation;

namespace TraceFlow.Api.Application.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Identifier)
        .NotEmpty()
        .MaximumLength(100);

        RuleFor(command => command.Password)
        .NotEmpty()
        .MaximumLength(100);
    }
}